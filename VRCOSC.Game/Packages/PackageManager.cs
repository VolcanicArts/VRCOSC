// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Packages.Serialisation;
using VRCOSC.Game.Screens.Loading;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Packages;

public class PackageManager
{
    private const string github_token = "";
    public static readonly GitHubClient GITHUB_CLIENT = new(new ProductHeaderValue("VRCOSC"), new InMemoryCredentialStore(new Credentials(github_token)));

    private const string community_tag = "vrcosc";
    private readonly Storage storage;
    private readonly SerialisationManager serialisationManager;

    public Action<LoadingInfo>? Progress;

    private readonly List<PackageSource> builtinSources = new();

    public readonly List<PackageSource> Sources = new();
    public readonly Dictionary<string, string> InstalledPackages = new();

    public DateTime CacheExpireTime = DateTime.UnixEpoch;

    public PackageManager(Storage baseStorage)
    {
        storage = baseStorage.GetStorageForDirectory("packages/remote");

        builtinSources.Add(new PackageSource(this, "VolcanicArts", "VRCOSC-OfficialModules", PackageType.Official));
        builtinSources.Add(new PackageSource(this, "DJDavid98", "VRCOSC-BluetoothHeartrate", PackageType.Curated));

        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new PackageManagerSerialiser(baseStorage, this));
    }

    public async Task Load()
    {
        serialisationManager.Deserialise();
        await RefreshAllSources(CacheExpireTime + TimeSpan.FromDays(1) <= DateTime.Now);
    }

    public async Task RefreshAllSources(bool forceRemoteGrab)
    {
        if (forceRemoteGrab)
        {
            Sources.Clear();
            Sources.AddRange(builtinSources);
            Sources.AddRange(await loadCommunityPackages());
        }

        foreach (var remoteModuleSource in Sources)
        {
            await remoteModuleSource.Refresh(forceRemoteGrab);
        }

        CacheExpireTime = DateTime.Now + TimeSpan.FromDays(1);
        serialisationManager.Serialise();
    }

    public async Task InstallPackage(PackageSource packageSource)
    {
        storage.DeleteDirectory(packageSource.PackageID);

        var installDirectory = storage.GetStorageForDirectory(packageSource.PackageID);
        var installAssets = packageSource.GetAssets();
        installAssets.Remove("vrcosc.json");

        foreach (var assetName in installAssets)
        {
            var assetDownload = new FileWebRequest(installDirectory.GetFullPath(assetName), $"{packageSource.DownloadURL}/{assetName}");
            await assetDownload.PerformAsync();
        }

        InstalledPackages[packageSource.PackageID!] = packageSource.LatestVersion!;
        serialisationManager.Serialise();
    }

    public void UninstallPackage(PackageSource packageSource)
    {
        InstalledPackages.Remove(packageSource.PackageID!);
        storage.DeleteDirectory(packageSource.PackageID);
        serialisationManager.Serialise();
    }

    public bool IsInstalled(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.ContainsKey(packageSource.PackageID);
    public string GetInstalledVersion(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.TryGetValue(packageSource.PackageID, out var version) ? version : string.Empty;

    private async Task<List<PackageSource>> loadCommunityPackages()
    {
        var packageSources = new List<PackageSource>();

        Logger.Log("Attempting to load community repos");

        var repos = await GITHUB_CLIENT.Search.SearchRepo(new SearchRepositoriesRequest
        {
            Topic = community_tag,
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
