// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace VRCOSC.Game.Modules.Remote;

public class RemoteModuleSourceManager
{
    private const string github_token = "";
    private const string community_tag = "vrcosc";

    private readonly GitHubClient gitHubClient = new(new ProductHeaderValue("VRCOSC"), new InMemoryCredentialStore(new Credentials(github_token)));
    private readonly Storage storage;

    private readonly Dictionary<string, RemoteModuleSource> sources = new()
    {
        { "VolcanicArts#VRCOSC-OfficialModules", new RemoteModuleSource("VolcanicArts", "VRCOSC-OfficialModules", RemoteModuleSourceType.Official) },
        { "DJDavid98#VRCOSC-BluetoothHeartrate", new RemoteModuleSource("DJDavid98", "VRCOSC-BluetoothHeartrate", RemoteModuleSourceType.Curated) }
    };

    public IReadOnlyList<RemoteModuleSource> Sources => sources.Values.ToList();

    /// <summary>
    /// A callback for any actions this <see cref="RemoteModuleSourceManager"/> may be executing
    /// </summary>
    public Action<LoadingInfo>? Progress;

    public RemoteModuleSourceManager(Storage storage)
    {
        this.storage = storage.GetStorageForDirectory("modules/remote");
    }

    /// <summary>
    /// Refreshes the listing
    /// </summary>
    public async Task Refresh()
    {
        Progress?.Invoke(new LoadingInfo("Beginning refresh", 0f, false));

        var divisor = 1f / sources.Count;
        var currentProgress = 0f;

        foreach (var remoteModuleSource in Sources)
        {
            Progress?.Invoke(new LoadingInfo($"Refreshing {remoteModuleSource.Identifier}", currentProgress, false));
            await remoteModuleSource.UpdateRemoteState(false);
            currentProgress += divisor;
        }

        Progress?.Invoke(new LoadingInfo("Finished!", 1f, true));
    }

    /// <summary>
    /// Loads all remote module sources and executes update checks
    /// </summary>
    public async Task Load()
    {
        Progress?.Invoke(new LoadingInfo("Loading remote modules", 0f, false));
        loadInstalledModules();
        await loadCommunityModuleSources();

        var divisor = 1f / sources.Count;
        var currentProgress = 0f;

        foreach (var remoteModuleSource in sources.Values)
        {
            remoteModuleSource.InjectDependencies(storage, gitHubClient);
            await remoteModuleSource.UpdateStates();

            currentProgress += divisor;
            Progress?.Invoke(new LoadingInfo($"Loading {remoteModuleSource.Identifier}", currentProgress, false));
        }

        var installedWithNoRelease = sources.Values.Where(remoteModuleSource => remoteModuleSource is { RemoteState: RemoteModuleSourceRemoteState.MissingLatestRelease, InstallState: RemoteModuleSourceInstallState.Valid });
        var installedWithNoRepo = sources.Values.Where(remoteModuleSource => remoteModuleSource is { RemoteState: RemoteModuleSourceRemoteState.Unknown, InstallState: RemoteModuleSourceInstallState.Valid });
        Progress?.Invoke(new LoadingInfo("Finished!", 1f, true));
    }

    private void loadInstalledModules()
    {
        storage.GetDirectories(string.Empty).ForEach(directoryName =>
        {
            var repoOwner = directoryName.Split('#', 2)[0];
            var repoName = directoryName.Split('#', 2)[1];
            var remoteModuleSource = new RemoteModuleSource(repoOwner, repoName, RemoteModuleSourceType.Community);
            sources.TryAdd(remoteModuleSource.Identifier, remoteModuleSource);
        });
    }

    private async Task loadCommunityModuleSources()
    {
        Logger.Log("Attempting to load community repos");

        var repos = await gitHubClient.Search.SearchRepo(new SearchRepositoriesRequest
        {
            Topic = community_tag,
        });

        Logger.Log($"Found {repos.TotalCount} community repos");

        repos.Items.Where(repo => repo.Name != "VRCOSC").ForEach(repo =>
        {
            var remoteModuleSource = new RemoteModuleSource(repo.Owner.HtmlUrl.Split('/').Last(), repo.Name, RemoteModuleSourceType.Community);
            sources.TryAdd(remoteModuleSource.Identifier, remoteModuleSource);
        });
    }
}
