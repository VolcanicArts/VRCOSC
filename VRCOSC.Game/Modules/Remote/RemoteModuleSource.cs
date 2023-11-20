// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Semver;

namespace VRCOSC.Game.Modules.Remote;

public class RemoteModuleSource
{
    private Storage storage = null!;
    private GitHubClient githubClient = null!;

    private readonly HttpClient httpClient = new();
    public readonly string RepositoryOwner;
    public readonly string RepositoryName;

    public Repository? Repository { get; private set; }
    public Release? LatestRelease { get; private set; }
    private DefinitionFile? latestReleaseDefinition;

    public readonly RemoteModuleSourceType SourceType;

    /// <summary>
    /// The remote state of the remote module source as per the last <see cref="UpdateRemoteState"/> call
    /// </summary>
    public RemoteModuleSourceRemoteState RemoteState { get; private set; } = RemoteModuleSourceRemoteState.Unknown;

    /// <summary>
    /// The install state of the remote module source as per the last <see cref="UpdateInstallState"/> call
    /// </summary>
    public RemoteModuleSourceInstallState InstallState { get; private set; } = RemoteModuleSourceInstallState.Unknown;

    /// <summary>
    /// The identifier of this remote module source.
    /// Used for the storage of the installed files.
    /// </summary>
    public string? PackageID => latestReleaseDefinition?.PackageID;

    public string InternalReference => $"{RepositoryOwner}#{RepositoryName}";

    /// <summary>
    /// The display name if defined in <see cref="latestReleaseDefinition"/> else the <see cref="RepositoryName"/>
    /// </summary>
    public string DisplayName => latestReleaseDefinition?.DisplayName ?? RepositoryName;

    /// <summary>
    /// A hook for getting data out of what this <see cref="RemoteModuleSource"/> is doing
    /// </summary>
    public Action<LoadingInfo>? Progress;

    public RemoteModuleSource(string repositoryOwner, string repositoryName, RemoteModuleSourceType sourceType)
    {
        RepositoryOwner = repositoryOwner;
        RepositoryName = repositoryName;
        SourceType = sourceType;
    }

    public void InjectDependencies(Storage storage, GitHubClient githubClient)
    {
        this.storage = storage;
        this.githubClient = githubClient;
    }

    public string GetInstalledVersion()
    {
        if (!IsInstalled()) return string.Empty;

        var metadataContents = File.ReadAllText(getLocalStorage().GetFullPath("metadata.json"));
        var metadata = JsonConvert.DeserializeObject<MetadataFile>(metadataContents);
        return metadata!.InstalledVersion;
    }

    public string? GetCoverUrl() => latestReleaseDefinition?.CoverImageUrl;

    private static SemVersion getCurrentSDKVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();
        return new SemVersion(version.Major, version.Minor, version.Build);
    }

    private Storage getLocalStorage() => storage.GetStorageForDirectory(PackageID);

    private void log(string message) => Logger.Log($"[{InternalReference}]: {message}");

    public bool IsIncompatible() => RemoteState is RemoteModuleSourceRemoteState.InvalidDefinitionFile or RemoteModuleSourceRemoteState.MissingDefinitionFile or RemoteModuleSourceRemoteState.SDKIncompatible;
    public bool IsUnavailable() => RemoteState is RemoteModuleSourceRemoteState.MissingLatestRelease or RemoteModuleSourceRemoteState.Unknown;
    public bool IsAvailable() => RemoteState is RemoteModuleSourceRemoteState.Valid;

    public bool IsInstalled() => InstallState is RemoteModuleSourceInstallState.Valid;

    /// <summary>
    /// Downloads all the files as specified in <see cref="DefinitionFile"/>
    /// </summary>
    public async Task Install()
    {
        Progress?.Invoke(new LoadingInfo($"Installing {InternalReference}", 0f, false));

        log($"Attempting to install repo {InternalReference}");

        if (RemoteState != RemoteModuleSourceRemoteState.Valid)
            throw new InvalidOperationException($"Cannot install when remote state is not {RemoteModuleSourceRemoteState.Valid}");

        Debug.Assert(LatestRelease is not null);
        Debug.Assert(latestReleaseDefinition is not null);

        storage.DeleteDirectory(PackageID);

        try
        {
            var localStorage = getLocalStorage();
            var assetsToDownload = LatestRelease.Assets.Where(releaseAsset => latestReleaseDefinition.Files.Contains(releaseAsset.Name)).ToList();

            var divisor = 1f / assetsToDownload.Count;
            var count = 0f;

            foreach (var releaseAsset in assetsToDownload)
            {
                var downloadRequest = new FileWebRequest(localStorage.GetFullPath(releaseAsset.Name), releaseAsset.BrowserDownloadUrl);
                log($"Downloading file {releaseAsset.Name}");
                Progress?.Invoke(new LoadingInfo("Downloading required files", count, false));
                await downloadRequest.PerformAsync();
                count += divisor;
            }

            var metadata = new MetadataFile
            {
                InstalledVersion = LatestRelease.TagName,
                RepoOwner = RepositoryOwner,
                RepoName = RepositoryName
            };

            Progress?.Invoke(new LoadingInfo("Writing metadata", 0f, false));

            using (var writeStream = localStorage.CreateFileSafely("metadata.json"))
            {
                using var writer = new StreamWriter(writeStream);
                await writer.WriteAsync(JsonConvert.SerializeObject(metadata));
            }

            log("Install successful");
            UpdateInstallState();

            Progress?.Invoke(new LoadingInfo("Finished!", 1f, true));
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Could not install repo {InternalReference}");
        }
    }

    public void Uninstall()
    {
        Progress?.Invoke(new LoadingInfo("Uninstalling", 0f, false));

        log("Attempting to uninstall");

        if (!storage.ExistsDirectory(PackageID))
        {
            log("Module is not installed. Skipping uninstallation");
            return;
        }

        storage.DeleteDirectory(PackageID);
        log("Uninstall successful");
        UpdateInstallState();

        Progress?.Invoke(new LoadingInfo("Finished!", 1f, true));
    }

    public async Task UpdateStates()
    {
        await UpdateRemoteState();
        UpdateInstallState();
    }

    /// <summary>
    /// Checks if an update is available.
    /// </summary>
    public bool IsUpdateAvailable()
    {
        if (IsUnavailable() || IsIncompatible() || !IsInstalled()) return false;

        var localStorage = getLocalStorage();
        var metadataContents = File.ReadAllText(localStorage.GetFullPath("metadata.json"));
        var metadata = JsonConvert.DeserializeObject<MetadataFile>(metadataContents)!;

        var installedVersion = SemVersion.Parse(metadata.InstalledVersion, SemVersionStyles.Any);
        var latestVersion = SemVersion.Parse(LatestRelease!.TagName, SemVersionStyles.Any);

        return installedVersion.ComparePrecedenceTo(latestVersion) < 0;
    }

    /// <summary>
    /// Checks the install state of the repo.
    /// Ensures that the repo directory exists.
    /// Ensures that metadata.json exists
    /// Ensures that metadata.json is formatted correctly.
    /// </summary>
    public void UpdateInstallState()
    {
        log("Updating install state");

        try
        {
            if (PackageID is null)
            {
                InstallState = RemoteModuleSourceInstallState.NotInstalled;
                return;
            }

            if (!storage.ExistsDirectory(PackageID))
            {
                InstallState = RemoteModuleSourceInstallState.NotInstalled;
                return;
            }

            var localStorage = getLocalStorage();

            if (!localStorage.Exists("metadata.json"))
            {
                InstallState = RemoteModuleSourceInstallState.Broken;
                return;
            }

            var metadataContents = File.ReadAllText(localStorage.GetFullPath("metadata.json"));
            var metadata = JsonConvert.DeserializeObject<MetadataFile>(metadataContents);

            if (metadata is null)
            {
                InstallState = RemoteModuleSourceInstallState.Broken;
                return;
            }

            InstallState = RemoteModuleSourceInstallState.Valid;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error when checking install state of repo {InternalReference}");
            InstallState = RemoteModuleSourceInstallState.Unknown;
        }
        finally
        {
            log($"Install state: {InstallState}");
        }
    }

    /// <summary>
    /// Checks the latest release of the repo to ensure it has all the prerequisites of being VRCOSC compatible.
    /// Ensures that a latest release exists.
    /// Ensures that vrcosc.json exists.
    /// Ensures that vrcosc.json is formatted correctly.
    /// Ensures that the remote source is compatible with the current SDK version
    /// <param name="useCache">Whether to use the cached latest release. Set to false to force a new API request</param>
    /// </summary>
    public async Task UpdateRemoteState(bool useCache = true)
    {
        log("Updating remote state");

        try
        {
            try
            {
                if (LatestRelease is null || !useCache)
                {
                    LatestRelease = await githubClient.Repository.Release.GetLatest(RepositoryOwner, RepositoryName);
                }

                if (Repository is null || !useCache)
                {
                    Repository = await githubClient.Repository.Get(RepositoryOwner, RepositoryName);
                }
            }
            catch (ApiException)
            {
                log("Could not retrieve latest release");
                RemoteState = RemoteModuleSourceRemoteState.MissingLatestRelease;
                return;
            }

            var definitionFileAsset = LatestRelease.Assets.SingleOrDefault(asset => asset.Name == "vrcosc.json");

            if (definitionFileAsset is null)
            {
                RemoteState = RemoteModuleSourceRemoteState.MissingDefinitionFile;
                return;
            }

            var definitionFileContents = await (await httpClient.GetAsync(definitionFileAsset.BrowserDownloadUrl)).Content.ReadAsStringAsync();

            try
            {
                latestReleaseDefinition = JsonConvert.DeserializeObject<DefinitionFile>(definitionFileContents);
            }
            catch (JsonException e)
            {
                Logger.Error(e, $"Error when deserialising contents of vrcosc.json for repo {InternalReference}");
                RemoteState = RemoteModuleSourceRemoteState.InvalidDefinitionFile;
                return;
            }

            if (latestReleaseDefinition is null)
            {
                RemoteState = RemoteModuleSourceRemoteState.InvalidDefinitionFile;
                return;
            }

            if (string.IsNullOrEmpty(latestReleaseDefinition.PackageID))
            {
                RemoteState = RemoteModuleSourceRemoteState.InvalidDefinitionFile;
                return;
            }

            var currentSDKVersion = getCurrentSDKVersion();
            var remoteModuleVersion = SemVersionRange.Parse(latestReleaseDefinition.SDKVersionRange, SemVersionRangeOptions.Loose);

            if (!currentSDKVersion.Satisfies(remoteModuleVersion))
            {
                RemoteState = RemoteModuleSourceRemoteState.SDKIncompatible;
                return;
            }

            RemoteState = RemoteModuleSourceRemoteState.Valid;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Problem when checking latest release for repo {InternalReference}");
            RemoteState = RemoteModuleSourceRemoteState.Unknown;
            LatestRelease = null;
            latestReleaseDefinition = null;
        }
        finally
        {
            log($"Remote state: {RemoteState}");
        }
    }
}

public enum RemoteModuleSourceRemoteState
{
    Unknown,
    MissingLatestRelease,
    MissingDefinitionFile,
    InvalidDefinitionFile,
    SDKIncompatible,
    Valid
}

public enum RemoteModuleSourceInstallState
{
    Unknown,
    NotInstalled,
    Broken,
    Valid
}

public enum RemoteModuleSourceType
{
    Official,
    Curated,
    Community
}
