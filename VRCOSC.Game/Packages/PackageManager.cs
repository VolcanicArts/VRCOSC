// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Actions.Packages;
using VRCOSC.Game.Config;
using VRCOSC.Game.Packages.Serialisation;
using VRCOSC.Game.Screens.Loading;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Packages;

public class PackageManager
{
    private const string community_tag = "vrcosc-package";
    private readonly VRCOSCGame game;
    private readonly AppManager appManager;
    private readonly VRCOSCConfigManager configManager;
    private readonly Storage storage;
    private readonly SerialisationManager serialisationManager;

    public Action<LoadingInfo>? Progress;

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

    public async Task Load()
    {
        Sources.AddRange(builtinSources);
        serialisationManager.Deserialise();
        await RefreshAllSources(CacheExpireTime + TimeSpan.FromDays(1) <= DateTime.Now);
    }

    public async Task RefreshAllSources(bool forceRemoteGrab)
    {
        Progress?.Invoke(new LoadingInfo("Refreshing all packages", 0f, false));

        if (forceRemoteGrab)
        {
            Sources.Clear();
            Sources.AddRange(builtinSources);
            Sources.AddRange(await loadCommunityPackages());
        }

        var divisor = 1f / Sources.Count;
        var count = 0f;

        foreach (var packageSource in Sources)
        {
            Progress?.Invoke(new LoadingInfo($"Refreshing {packageSource.GetDisplayName()}", count, false));
            await packageSource.Refresh(forceRemoteGrab, configManager.Get<bool>(VRCOSCSetting.AllowPreReleasePackages));
            count += divisor;
        }

        CacheExpireTime = DateTime.Now + TimeSpan.FromDays(1);
        serialisationManager.Serialise();

        Progress?.Invoke(new LoadingInfo("Complete!", 1f, true));
    }

    public async Task InstallPackage(PackageSource packageSource)
    {
        var isInstalled = InstalledPackages.ContainsKey(packageSource.PackageID!);
        var installAction = new PackageInstallAction(storage, packageSource, isInstalled);
        game.LoadingScreen.CurrentAction.Value = installAction;

        await installAction.Execute();
        appManager.ModuleManager.ReloadAllModules();

        InstalledPackages[packageSource.PackageID!] = packageSource.LatestVersion!;
        serialisationManager.Serialise();

        game.OnListingRefresh?.Invoke();
    }

    public async Task UninstallPackage(PackageSource packageSource)
    {
        var uninstallAction = new PackageUninstallAction(storage, packageSource);
        game.LoadingScreen.CurrentAction.Value = uninstallAction;
        await uninstallAction.Execute();
        appManager.ModuleManager.ReloadAllModules();

        InstalledPackages.Remove(packageSource.PackageID!);
        serialisationManager.Serialise();

        game.OnListingRefresh?.Invoke();
    }

    public bool IsInstalled(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.ContainsKey(packageSource.PackageID);
    public string GetInstalledVersion(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.TryGetValue(packageSource.PackageID, out var version) ? version : string.Empty;

    private async Task<List<PackageSource>> loadCommunityPackages()
    {
        Progress?.Invoke(new LoadingInfo("Retrieving community packages", 0f, false));

        var packageSources = new List<PackageSource>();

        Logger.Log("Attempting to load community repos");

        var repos = await GitHubProxy.Client.Search.SearchRepo(new SearchRepositoriesRequest
        {
            Topic = community_tag
        });

        Logger.Log($"Found {repos.TotalCount} community repos");

        repos.Items.Where(repo => repo.Name != "VRCOSC").ForEach(repo =>
        {
            var packageSource = new PackageSource(this, repo.Owner.HtmlUrl.Split('/').Last(), repo.Name);
            if (builtinSources.Any(comparedSource => comparedSource.InternalReference == packageSource.InternalReference)) return;

            packageSources.Add(packageSource);
        });

        return packageSources;
    }
}
