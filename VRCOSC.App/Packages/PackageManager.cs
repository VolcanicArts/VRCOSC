// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VRCOSC.App.Actions;
using VRCOSC.App.Actions.Packages;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages.Serialisation;
using VRCOSC.App.Serialisation;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;

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

    public DateTime CacheExpireTime = DateTime.UnixEpoch;

    public PackageSource OfficialModulesSource { get; }

    public PackageManager()
    {
        var baseStorage = AppManager.GetInstance().Storage;
        storage = baseStorage.GetStorageForDirectory("packages/remote");

        builtinSources.Add(OfficialModulesSource = new PackageSource(this, "VolcanicArts", "VRCOSC-Modules", PackageType.Official));
        builtinSources.Add(new PackageSource(this, "DJDavid98", "VRCOSC-BluetoothHeartrate", PackageType.Curated));
        builtinSources.Add(new PackageSource(this, "TahvoDev", "AxHaptics", PackageType.Curated));

        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new PackageManagerSerialiser(baseStorage, this));
    }

    public PackageSource? GetPackage(string packageID) => Sources.FirstOrDefault(packageSource => packageSource.PackageID == packageID);
    public PackageSource? GetPackageSourceForRelease(PackageRelease packageRelease) => Sources.FirstOrDefault(packageSource => packageSource.FilteredReleases.Contains(packageRelease));

    public PackageLoadAction Load()
    {
        builtinSources.ForEach(source => Sources.Add(source));
        serialisationManager.Deserialise();
        return RefreshAllSources(CacheExpireTime <= DateTime.Now);
    }

    public PackageLoadAction RefreshAllSources(bool forceRemoteGrab)
    {
        var packageLoadAction = new PackageLoadAction();

        if (forceRemoteGrab)
        {
            packageLoadAction.AddAction(new DynamicProgressAction("Loading built-in packages", () =>
            {
                Sources.Clear();
                builtinSources.ForEach(source => Sources.Add(source));
            }));
            packageLoadAction.AddAction(loadCommunityPackages());
        }

        packageLoadAction.AddAction(new PackagesRefreshAction(Sources, forceRemoteGrab));

        packageLoadAction.OnComplete += () =>
        {
            if (forceRemoteGrab) CacheExpireTime = DateTime.Now + TimeSpan.FromDays(1);
            serialisationManager.Serialise();
            MainWindow.GetInstance().PackagesView.Refresh();
        };

        return packageLoadAction;
    }

    public PackageInstallAction InstallPackage(PackageSource packageSource, PackageRelease? packageRelease = null)
    {
        packageRelease ??= packageSource.LatestRelease;
        var isInstalled = InstalledPackages.ContainsKey(packageSource.PackageID!);
        var installAction = new PackageInstallAction(storage, packageSource, packageRelease, isInstalled);

        installAction.OnComplete += () =>
        {
            InstalledPackages[packageSource.PackageID!] = packageRelease.Version;
            serialisationManager.Serialise();
            ModuleManager.GetInstance().ReloadAllModules();
            MainWindow.GetInstance().PackagesView.Refresh();
        };

        return installAction;
    }

    public PackageUninstallAction UninstallPackage(PackageSource packageSource)
    {
        var uninstallAction = new PackageUninstallAction(storage, packageSource);

        uninstallAction.OnComplete += () =>
        {
            InstalledPackages.Remove(packageSource.PackageID!);
            serialisationManager.Serialise();
            ModuleManager.GetInstance().ReloadAllModules();
            MainWindow.GetInstance().PackagesView.Refresh();
        };

        return uninstallAction;
    }

    public bool IsInstalled(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.ContainsKey(packageSource.PackageID);
    public string GetInstalledVersion(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.TryGetValue(packageSource.PackageID, out var version) ? version : string.Empty;

    private FindCommunityPackagesAction loadCommunityPackages()
    {
        var findCommunityPackages = new FindCommunityPackagesAction();

        var searchProgressAction = new SearchRepositoriesAction(community_tag);
        findCommunityPackages.AddAction(searchProgressAction);

        findCommunityPackages.AddAction(new DynamicProgressAction("Auditing community packages", () =>
        {
            var repos = searchProgressAction.Result!;

            repos.Items.Where(repo => repo.Name != "VRCOSC").ToList().ForEach(repo =>
            {
                var packageSource = new PackageSource(this, repo.Owner.HtmlUrl.Split('/').Last(), repo.Name);
                if (builtinSources.Any(comparedSource => comparedSource.InternalReference == packageSource.InternalReference)) return;

                Sources.Add(packageSource);
            });
        }));

        return findCommunityPackages;
    }
}
