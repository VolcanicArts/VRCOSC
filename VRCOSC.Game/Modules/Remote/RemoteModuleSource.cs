// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Semver;

namespace VRCOSC.Game.Modules.Remote;

public class RemoteModuleSource
{
    private Storage storage = null!;

    private readonly HttpClient httpClient = new();
    private readonly GitHubClient githubClient = new(new ProductHeaderValue("VRCOSC"));
    private readonly string repositoryOwner;
    private readonly string repositoryName;

    private Release? latestRelease;

    /// <summary>
    /// The remote state of the remote module source as per the last <see cref="UpdateRemoteState"/> call
    /// </summary>
    public RemoteModuleSourceRemoteState RemoteState { get; private set; } = RemoteModuleSourceRemoteState.Unknown;

    /// <summary>
    /// The install state of the remote module source as per the last <see cref="UpdateInstallState"/> call
    /// </summary>
    public RemoteModuleSourceInstallState InstallState { get; private set; } = RemoteModuleSourceInstallState.Unknown;

    /// <summary>
    /// The formatted identifier of this remote module source.
    /// Used for the storage of the installed files
    /// </summary>
    public string FormattedIdentifier => $"{repositoryOwner}#{repositoryName}";

    public RemoteModuleSource(string repositoryOwner, string repositoryName)
    {
        this.repositoryOwner = repositoryOwner;
        this.repositoryName = repositoryName;
    }

    public void InjectDependencies(Storage storage)
    {
        this.storage = storage.GetStorageForDirectory("modules/remote");
    }

    private SemVersion getCurrentSDKVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version!;
        return new SemVersion(version.Major, version.Minor, version.Build);
    }

    /// <summary>
    /// Downloads all the files as specified in <see cref="DefinitionFile"/>
    /// <param name="forceInstall">Set to true when wanting to install the latest version even if it's already installed</param>
    /// </summary>
    public async Task Install(bool forceInstall = false)
    {
        if (RemoteState != RemoteModuleSourceRemoteState.Valid)
            throw new InvalidOperationException($"Cannot install when remote state is not {RemoteModuleSourceRemoteState.Valid}");
    }

    /// <summary>
    /// Checks if an update is available.
    /// </summary>
    public async Task<bool> IsUpdateAvailable()
    {
        await UpdateInstallState();
        await UpdateRemoteState();

        if (InstallState != RemoteModuleSourceInstallState.Valid)
            throw new InvalidOperationException($"Cannot check for available update when install state is not {RemoteModuleSourceInstallState.Valid}");

        if (RemoteState != RemoteModuleSourceRemoteState.Valid)
            throw new InvalidOperationException($"Cannot check for available update when remote state is not {RemoteModuleSourceRemoteState.Valid}");

        var localStorage = storage.GetStorageForDirectory(FormattedIdentifier);
        var metadataContents = await File.ReadAllTextAsync(localStorage.GetFullPath("metadata.json"));
        var metadata = JsonConvert.DeserializeObject<MetadataFile>(metadataContents)!;

        var installedVersion = SemVersion.Parse(metadata.InstalledVersion, SemVersionStyles.Any);
        var latestVersion = SemVersion.Parse(latestRelease!.TagName, SemVersionStyles.Any);

        return installedVersion.ComparePrecedenceTo(latestVersion) < 0;
    }

    /// <summary>
    /// Checks the install state of the repo.
    /// Ensures that the repo directory exists.
    /// Ensures that metadata.json exists
    /// Ensures that metadata.json is formatted correctly.
    /// </summary>
    public async Task UpdateInstallState()
    {
        try
        {
            if (!storage.ExistsDirectory(FormattedIdentifier))
            {
                InstallState = RemoteModuleSourceInstallState.NotInstalled;
                return;
            }

            var localStorage = storage.GetStorageForDirectory(FormattedIdentifier);

            if (!localStorage.Exists("metadata.json"))
            {
                InstallState = RemoteModuleSourceInstallState.Broken;
                return;
            }

            var metadataContents = await File.ReadAllTextAsync(localStorage.GetFullPath("metadata.json"));
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
            Logger.Error(e, $"Error when checking install state of repo {FormattedIdentifier}");
            InstallState = RemoteModuleSourceInstallState.Unknown;
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
        try
        {
            try
            {
                if (latestRelease is null || !useCache)
                    latestRelease = await githubClient.Repository.Release.GetLatest(repositoryOwner, repositoryName);
            }
            catch (ApiException e)
            {
                Logger.Error(e, $"Could not retrieve latest release for repository {FormattedIdentifier}");
                RemoteState = RemoteModuleSourceRemoteState.MissingLatestRelease;
                return;
            }

            var definitionFileAsset = latestRelease.Assets.SingleOrDefault(asset => asset.Name == "vrcosc.json");

            if (definitionFileAsset is null)
            {
                RemoteState = RemoteModuleSourceRemoteState.MissingDefinitionFile;
                return;
            }

            var definitionFileContents = await (await httpClient.GetAsync(definitionFileAsset.BrowserDownloadUrl)).Content.ReadAsStringAsync();
            var definitionFile = JsonConvert.DeserializeObject<DefinitionFile>(definitionFileContents);

            if (definitionFile is null)
            {
                RemoteState = RemoteModuleSourceRemoteState.InvalidDefinitionFile;
                return;
            }

            var currentSDKVersion = getCurrentSDKVersion();
            var remoteModuleVersion = SemVersionRange.Parse(definitionFile.SDKVersionRange, SemVersionRangeOptions.Loose);

            if (!currentSDKVersion.Satisfies(remoteModuleVersion))
            {
                RemoteState = RemoteModuleSourceRemoteState.SDKIncompatible;
                return;
            }

            RemoteState = RemoteModuleSourceRemoteState.Valid;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Problem when checking latest release for repo {FormattedIdentifier}");
            RemoteState = RemoteModuleSourceRemoteState.Unknown;
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
