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
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Packages;

// Yeusepe
public class PackageManager
{
    private static PackageManager? instance;
    public static PackageManager GetInstance() => instance ??= new PackageManager();

    private const string community_tag = "vrcosc-package";

    private readonly Storage storage;
    private readonly SerialisationManager serialisationManager;

    private readonly List<PackageSource> builtinSources = new();

    public ObservableCollection<PackageSource> Sources { get; } = new();

    // id - version
    public readonly Dictionary<string, string> InstalledPackages = new();

    public DateTime CacheExpireTime = DateTime.UnixEpoch;

    public PackageSource OfficialModulesSource { get; }

    // Yeusepe
    public event Action? PackagesUpdated;
    private void NotifyPackagesUpdated() => PackagesUpdated?.Invoke();

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

    public CompositeProgressAction Load()
    {
        builtinSources.ForEach(source => Sources.Add(source));
        serialisationManager.Deserialise();
        return RefreshAllSources(CacheExpireTime <= DateTime.Now);
    }

    public void Serialise() => serialisationManager.Serialise();

    public CompositeProgressAction RefreshAllSources(bool forceRemoteGrab)
    {
        var packageLoadAction = new CompositeProgressAction();

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
            NotifyPackagesUpdated(); // Notify UI
        };

        return packageLoadAction;
    }

    public CompositeProgressAction? UpdateAllInstalledPackages()
    {
        var compositeAction = new CompositeProgressAction();
        var shouldDoAction = false;

        foreach (var packageSource in Sources.Where(source => source.IsInstalled()))
        {
            var latestNonPreRelease = packageSource.GetLatestNonPreRelease();
            if (latestNonPreRelease is null) continue;

            compositeAction.AddAction(InstallPackage(packageSource, latestNonPreRelease, false, false));
            shouldDoAction = true;
        }

        if (!shouldDoAction) return null;

        compositeAction.OnComplete += () =>
        {
            serialisationManager.Serialise();
            NotifyPackagesUpdated(); // Notify UI
        };

        return compositeAction;
    }

    public PackageInstallAction InstallPackage(PackageSource packageSource, PackageRelease? packageRelease = null, bool reloadAll = true, bool refreshBeforeInstall = true)
    {
        var installAction = new PackageInstallAction(storage, packageSource, packageRelease, IsInstalled(packageSource), refreshBeforeInstall);

        installAction.OnComplete += () =>
        {
            InstalledPackages[packageSource.PackageID!] = packageRelease?.Version ?? packageSource.LatestVersion;

            if (reloadAll)
            {
                serialisationManager.Serialise();
                ModuleManager.GetInstance().ReloadAllModules();
                MainWindow.GetInstance().PackagesView.Refresh();
            }
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

    private CompositeProgressAction loadCommunityPackages()
    {
        var findCommunityPackages = new CompositeProgressAction();

        var searchProgressAction = new SearchRepositoriesAction(community_tag);
        findCommunityPackages.AddAction(searchProgressAction);

        findCommunityPackages.AddAction(new DynamicProgressAction("Auditing community packages", () =>
        {
            var repos = searchProgressAction.Result!;

            foreach (var repo in repos.Items.Where(repo => repo.Name != "VRCOSC"))
            {
                var packageSource = new PackageSource(this, repo.Owner.HtmlUrl.Split('/').Last(), repo.Name);
                if (builtinSources.Any(comparedSource => comparedSource.InternalReference == packageSource.InternalReference)) continue;

                Sources.Add(packageSource);
            }
        }));

        return findCommunityPackages;
    }
}