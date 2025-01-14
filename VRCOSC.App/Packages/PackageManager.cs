// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Octokit;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages.Serialisation;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;
using Application = System.Windows.Application;

namespace VRCOSC.App.Packages;

public class PackageManager
{
    private static PackageManager? instance;
    internal static PackageManager GetInstance() => instance ??= new PackageManager();

    private const string community_tag = "vrcosc-package";

    private readonly Storage storage;
    private readonly SerialisationManager serialisationManager;

    private readonly List<PackageSource> builtinSources = new();

    public ObservableCollection<PackageSource> Sources { get; } = new();

    // id - version
    public readonly Dictionary<string, string> InstalledPackages = new();

    public DateTimeOffset CacheExpireTime = DateTimeOffset.UnixEpoch;

    public PackageSource OfficialModulesSource { get; }

    public bool IsCacheOutdated => CacheExpireTime <= DateTimeOffset.Now;

    public PackageManager()
    {
        var baseStorage = AppManager.GetInstance().Storage;
        storage = baseStorage.GetStorageForDirectory("packages/remote");

        builtinSources.Add(OfficialModulesSource = new PackageSource(this, "VolcanicArts", "VRCOSC-Modules", PackageType.Official));
        builtinSources.Add(new PackageSource(this, "DJDavid98", "VRCOSC-BluetoothHeartrate", PackageType.Curated));

        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new PackageManagerSerialiser(baseStorage, this));

        SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AllowPreReleasePackages).Subscribe(() => MainWindow.GetInstance().PackagesView.Refresh());
    }

    public PackageSource? GetPackage(string packageID) => Sources.FirstOrDefault(packageSource => packageSource.PackageID == packageID);
    public PackageSource? GetPackageSourceForRelease(PackageRelease packageRelease) => Sources.FirstOrDefault(packageSource => packageSource.FilteredReleases.Contains(packageRelease));

    public async Task Load()
    {
        builtinSources.ForEach(source => Sources.Add(source));
        serialisationManager.Deserialise();
        await Task.WhenAll(Sources.Select(source => source.Refresh(false)));

        // TODO: Check that the packages actually exist on disk out of the installed list
        // if they don't exist on disk, re-download the specified installed version
        // would mean I don't have to backup the packages folder
    }

    public async Task RefreshAllSources(bool forceRemoteGrab)
    {
        if (forceRemoteGrab)
        {
            Sources.Clear();
            builtinSources.ForEach(source => Sources.Add(source));
            await loadCommunityPackages();
        }

        // TODO: Split into 5s/10s and request in those groups
        var tasks = Sources.Select(packageSource => packageSource.Refresh(forceRemoteGrab));
        await Task.WhenAll(tasks);

        if (forceRemoteGrab) CacheExpireTime = DateTime.Now + TimeSpan.FromDays(1);
        serialisationManager.Serialise();
        MainWindow.GetInstance().PackagesView.Refresh();
    }

    public bool AnyInstalledPackageUpdates() => Sources.Where(source => source.IsInstalled()).Any(packageSource => packageSource.GetLatestNonPreRelease() is not null);

    public async Task UpdateAllInstalledPackages()
    {
        var tasks = Sources.Where(source => source.IsInstalled()).Select(packageSource =>
        {
            var latestNonPreRelease = packageSource.GetLatestNonPreRelease();
            return latestNonPreRelease is null ? Task.CompletedTask : InstallPackage(packageSource, latestNonPreRelease, false, false);
        });

        await Task.WhenAll(tasks);

        serialisationManager.Serialise();
    }

    public async Task<bool> InstallPackage(PackageSource packageSource, PackageRelease? packageRelease = null, bool reloadAll = true, bool refreshBeforeInstall = true, bool closeWindows = true)
    {
        if (!packageSource.IsAvailable()) return false;

        if (closeWindows)
        {
            foreach (var window in Application.Current.Windows.OfType<Window>().Where(w => w != Application.Current.MainWindow))
            {
                window.Close();
            }
        }

        if (IsInstalled(packageSource)) storage.DeleteDirectory(packageSource.PackageID!);
        if (refreshBeforeInstall) await packageSource.Refresh(true);

        var targetDirectory = storage.GetStorageForDirectory(packageSource.PackageID!);
        var release = packageRelease ?? packageSource.LatestRelease;

        Logger.Log($"Installing {packageSource.InternalReference}");

        await downloadRelease(packageSource, release, targetDirectory);

        InstalledPackages[packageSource.PackageID!] = packageRelease?.Version ?? packageSource.LatestVersion;

        if (reloadAll)
        {
            serialisationManager.Serialise();
            await ModuleManager.GetInstance().ReloadAllModules();
            MainWindow.GetInstance().PackagesView.Refresh();
        }

        return true;
    }

    private async Task downloadRelease(PackageSource source, PackageRelease release, Storage targetDirectory)
    {
        var tasks = release.Assets.Select(assetName => Task.Run(async () =>
        {
            var fileDownload = new FileDownload();
            await fileDownload.DownloadFileAsync(new Uri($"{source.URL}/releases/download/{release.Version}/{assetName}"), targetDirectory.GetFullPath(assetName, true));

            if (assetName.EndsWith(".zip"))
            {
                var zipPath = targetDirectory.GetFullPath(assetName);
                var extractPath = targetDirectory.GetFullPath(string.Empty);

                ZipFile.ExtractToDirectory(zipPath, extractPath);

                targetDirectory.Delete(assetName);
            }
        }));

        await Task.WhenAll(tasks);
    }

    public async Task UninstallPackage(PackageSource packageSource)
    {
        foreach (var window in Application.Current.Windows.OfType<Window>().Where(w => w != Application.Current.MainWindow))
        {
            window.Close();
        }

        Logger.Log($"Uninstalling {packageSource.InternalReference}");

        storage.DeleteDirectory(packageSource.PackageID!);

        InstalledPackages.Remove(packageSource.PackageID!);
        serialisationManager.Serialise();
        await ModuleManager.GetInstance().ReloadAllModules();
        MainWindow.GetInstance().PackagesView.Refresh();
    }

    public bool IsInstalled(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.ContainsKey(packageSource.PackageID);
    public string GetInstalledVersion(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.TryGetValue(packageSource.PackageID, out var version) ? version : string.Empty;

    private async Task loadCommunityPackages()
    {
        var repos = await GitHubProxy.Client.Search.SearchRepo(new SearchRepositoriesRequest
        {
            Topic = community_tag,
            Fork = ForkQualifier.IncludeForks
        }).WaitAsync(TimeSpan.FromSeconds(5));

        foreach (var repo in repos.Items.Where(repo => repo.Name != "VRCOSC"))
        {
            var packageSource = new PackageSource(this, repo.Owner.HtmlUrl.Split('/').Last(), repo.Name);
            if (builtinSources.Any(comparedSource => comparedSource.InternalReference == packageSource.InternalReference)) continue;

            Sources.Add(packageSource);
        }
    }
}