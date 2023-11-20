// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

    public readonly List<RemoteModuleSource> Sources = new()
    {
        new RemoteModuleSource("VolcanicArts", "VRCOSC-OfficialModules", RemoteModuleSourceType.Official),
        new RemoteModuleSource("DJDavid98", "VRCOSC-BluetoothHeartrate", RemoteModuleSourceType.Curated)
    };

    /// <summary>
    /// A callback for any actions this <see cref="RemoteModuleSourceManager"/> may be executing
    /// </summary>
    public Action<LoadingInfo>? Progress;

    public RemoteModuleSourceManager(Storage storage)
    {
        this.storage = storage.GetStorageForDirectory("packages/remote");
    }

    /// <summary>
    /// Refreshes the listing
    /// </summary>
    public async Task Refresh()
    {
        Progress?.Invoke(new LoadingInfo("Beginning refresh", 0f, false));

        var divisor = 1f / Sources.Count;
        var currentProgress = 0f;

        foreach (var remoteModuleSource in Sources)
        {
            Progress?.Invoke(new LoadingInfo($"Refreshing {remoteModuleSource.InternalReference}", currentProgress, false));
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
        await loadInstalledModules();
        await loadCommunityModuleSources();

        var divisor = 1f / Sources.Count;
        var currentProgress = 0f;

        foreach (var remoteModuleSource in Sources)
        {
            remoteModuleSource.InjectDependencies(storage, gitHubClient);
            await remoteModuleSource.UpdateStates();

            currentProgress += divisor;
            Progress?.Invoke(new LoadingInfo($"Loading {remoteModuleSource.InternalReference}", currentProgress, false));
        }

        var installedWithNoRelease = Sources.Where(remoteModuleSource => remoteModuleSource is { RemoteState: RemoteModuleSourceRemoteState.MissingLatestRelease, InstallState: RemoteModuleSourceInstallState.Valid });
        var installedWithNoRepo = Sources.Where(remoteModuleSource => remoteModuleSource is { RemoteState: RemoteModuleSourceRemoteState.Unknown, InstallState: RemoteModuleSourceInstallState.Valid });
        Progress?.Invoke(new LoadingInfo("Finished!", 1f, true));
    }

    private async Task loadInstalledModules()
    {
        var directoryPath = storage.GetFullPath(string.Empty);

        foreach (var repoDirectoryPath in Directory.GetDirectories(directoryPath))
        {
            var metadataFile = JsonConvert.DeserializeObject<MetadataFile>(await File.ReadAllTextAsync(Path.Join(repoDirectoryPath, "metadata.json")));
            if (metadataFile is null) continue;

            var remoteModuleSource = new RemoteModuleSource(metadataFile.RepoOwner, metadataFile.RepoName, RemoteModuleSourceType.Community);

            if (Sources.All(comparedSource => comparedSource.InternalReference != remoteModuleSource.InternalReference))
            {
                Sources.Add(remoteModuleSource);
            }
        }
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

            if (Sources.All(comparedSource => comparedSource.InternalReference != remoteModuleSource.InternalReference))
            {
                Sources.Add(remoteModuleSource);
            }
        });
    }
}
