// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Actions;
using VRCOSC.Actions.Packages;
using VRCOSC.Config;
using VRCOSC.Packages.Serialisation;
using VRCOSC.Serialisation;

namespace VRCOSC.Packages;

public class PackageManager
{
    private const string community_tag = "vrcosc-package";
    private readonly VRCOSCGame game;
    private readonly AppManager appManager;
    private readonly VRCOSCConfigManager configManager;
    private readonly Storage storage;
    private readonly SerialisationManager serialisationManager;

    private readonly List<PackageSource> builtinSources = new();

    public readonly List<PackageSource> Sources = new();
    public readonly Dictionary<string, string> InstalledPackages = new();

    public DateTime CacheExpireTime = DateTime.UnixEpoch;

    public PackageManager(VRCOSCGame game, AppManager appManager, Storage baseStorage, VRCOSCConfigManager configManager)
    {
        this.game = game;
        this.appManager = appManager;
        this.configManager = configManager;
        storage = baseStorage.GetStorageForDirectory("packages/remote");

        builtinSources.Add(new PackageSource(this, "VolcanicArts", "VRCOSC-Modules", PackageType.Official));
        builtinSources.Add(new PackageSource(this, "DJDavid98", "VRCOSC-BluetoothHeartrate", PackageType.Curated));

        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new PackageManagerSerialiser(baseStorage, this));
    }

    public PackageLoadAction Load()
    {
        Sources.AddRange(builtinSources);
        serialisationManager.Deserialise();
        return RefreshAllSources(CacheExpireTime + TimeSpan.FromDays(1) <= DateTime.Now);
    }

    public PackageLoadAction RefreshAllSources(bool forceRemoteGrab)
    {
        var packageLoadAction = new PackageLoadAction();

        if (forceRemoteGrab)
        {
            packageLoadAction.AddAction(new DynamicProgressAction("Loading built-in packages", () =>
            {
                Sources.Clear();
                Sources.AddRange(builtinSources);
            }));
            packageLoadAction.AddAction(loadCommunityPackages());
        }

        packageLoadAction.AddAction(new PackagesRefreshAction(Sources, forceRemoteGrab, configManager.Get<bool>(VRCOSCSetting.AllowPreReleasePackages)));

        packageLoadAction.OnComplete += () =>
        {
            CacheExpireTime = DateTime.Now + TimeSpan.FromDays(1);
            serialisationManager.Serialise();
        };

        return packageLoadAction;
    }

    public PackageInstallAction InstallPackage(PackageSource packageSource)
    {
        var isInstalled = InstalledPackages.ContainsKey(packageSource.PackageID!);
        var installAction = new PackageInstallAction(storage, packageSource, isInstalled);

        installAction.OnComplete += () =>
        {
            InstalledPackages[packageSource.PackageID!] = packageSource.LatestVersion!;
            serialisationManager.Serialise();
            appManager.ModuleManager.ReloadAllModules();
            game.OnListingRefresh?.Invoke();
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
            appManager.ModuleManager.ReloadAllModules();
            game.OnListingRefresh?.Invoke();
        };

        return uninstallAction;
    }

    public bool IsInstalled(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.ContainsKey(packageSource.PackageID);
    public string GetInstalledVersion(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.TryGetValue(packageSource.PackageID, out var version) ? version : string.Empty;

    private FindCommunityPackagesAction loadCommunityPackages()
    {
        var findCommunityPackages = new FindCommunityPackagesAction();

        var packageSources = new List<PackageSource>();

        Logger.Log("Attempting to load community repos");

        var searchProgressAction = new SearchRepositoriesAction(community_tag);
        findCommunityPackages.AddAction(searchProgressAction);

        findCommunityPackages.AddAction(new DynamicProgressAction("Auditing community packages", () =>
        {
            var repos = searchProgressAction.Result!;
            Logger.Log($"Found {repos.TotalCount} community repos");

            repos.Items.Where(repo => repo.Name != "VRCOSC").ForEach(repo =>
            {
                var packageSource = new PackageSource(this, repo.Owner.HtmlUrl.Split('/').Last(), repo.Name);
                if (builtinSources.Any(comparedSource => comparedSource.InternalReference == packageSource.InternalReference)) return;

                packageSources.Add(packageSource);
            });
        }));

        return findCommunityPackages;
    }
}
