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
    public string Identifier => $"{RepositoryOwner}#{RepositoryName}";

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
        UpdateInstallState();
        if (InstallState != RemoteModuleSourceInstallState.Valid) return string.Empty;

        var metadataContents = File.ReadAllText(getLocalStorage().GetFullPath("metadata.json"));
        var metadata = JsonConvert.DeserializeObject<MetadataFile>(metadataContents);
        return metadata!.InstalledVersion;
    }

    private static SemVersion getCurrentSDKVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version!;
        return new SemVersion(version.Major, version.Minor, version.Build);
    }

    private Storage getLocalStorage() => storage.GetStorageForDirectory(Identifier);

    private void log(string message) => Logger.Log($"[{Identifier}]: {message}");

    /// <summary>
    /// Downloads all the files as specified in <see cref="DefinitionFile"/>
    /// </summary>
    /// <param name="forceInstall">Set to true when wanting to install the latest version even if it's already installed</param>
    public async Task Install(bool forceInstall = false)
    {
        log($"Attempting to install repo {Identifier}. forceInstall: {forceInstall}");

        if (RemoteState != RemoteModuleSourceRemoteState.Valid)
            throw new InvalidOperationException($"Cannot install when remote state is not {RemoteModuleSourceRemoteState.Valid}");

        Debug.Assert(LatestRelease is not null);
        Debug.Assert(latestReleaseDefinition is not null);

        if (InstallState == RemoteModuleSourceInstallState.Valid && !forceInstall)
        {
            log("Repo is already installed");
            return;
        }

        try
        {
            if (forceInstall)
            {
                log("Force install chosen. Attempting to uninstall first");
                Uninstall();
            }

            var localStorage = getLocalStorage();
            var assetsToDownload = LatestRelease.Assets.Where(releaseAsset => latestReleaseDefinition.Files.Contains(releaseAsset.Name));

            foreach (var releaseAsset in assetsToDownload)
            {
                var downloadRequest = new FileWebRequest(localStorage.GetFullPath(releaseAsset.Name), releaseAsset.BrowserDownloadUrl);
                log($"Downloading file {releaseAsset.Name}");
                await downloadRequest.PerformAsync();
            }

            var metadata = new MetadataFile
            {
                InstalledVersion = LatestRelease.TagName
            };

            using var writeStream = localStorage.CreateFileSafely("metadata.json");
            using var writer = new StreamWriter(writeStream);
            await writer.WriteAsync(JsonConvert.SerializeObject(metadata));

            log("Install successful");
            UpdateInstallState();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Could not install repo {Identifier}");
        }
    }

    public void Uninstall()
    {
        log($"Attempting to uninstall");

        if (!storage.ExistsDirectory(Identifier))
        {
            log("Module is not installed. Skipping uninstallation");
            return;
        }

        storage.DeleteDirectory(Identifier);
        log("Uninstall successful");
    }

    public async Task UpdateStates()
    {
        await UpdateRemoteState();
        UpdateInstallState();
    }

    /// <summary>
    /// Checks if an update is available.
    /// </summary>
    /// <param name="updateStates">Set to true to update the states before checking for updates</param>
    public async Task<bool> IsUpdateAvailable(bool updateStates = false)
    {
        log($"Checking for updates. updateStates: {updateStates}");

        if (updateStates)
        {
            await UpdateRemoteState();
            UpdateInstallState();
        }

        if (RemoteState == RemoteModuleSourceRemoteState.SDKIncompatible)
            return false;

        if (InstallState != RemoteModuleSourceInstallState.Valid)
            throw new InvalidOperationException($"Cannot check for available update when install state is not {RemoteModuleSourceInstallState.Valid}");

        if (RemoteState != RemoteModuleSourceRemoteState.Valid)
            throw new InvalidOperationException($"Cannot check for available update when remote state is not {RemoteModuleSourceRemoteState.Valid}");

        var localStorage = getLocalStorage();
        var metadataContents = await File.ReadAllTextAsync(localStorage.GetFullPath("metadata.json"));
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
            if (!storage.ExistsDirectory(Identifier))
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
            Logger.Error(e, $"Error when checking install state of repo {Identifier}");
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
                    LatestRelease = await githubClient.Repository.Release.GetLatest(RepositoryOwner, RepositoryName);
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
                Logger.Error(e, $"Error when deserialising contents of vrcosc.json for repo {Identifier}");
                RemoteState = RemoteModuleSourceRemoteState.InvalidDefinitionFile;
                return;
            }

            if (latestReleaseDefinition is null)
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
            Logger.Error(e, $"Problem when checking latest release for repo {Identifier}");
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
