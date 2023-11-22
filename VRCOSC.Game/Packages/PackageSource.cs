// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using osu.Framework.Logging;
using Semver;
using VRCOSC.Game.Packages.Sources;
using VRCOSC.Game.Screens.Loading;

namespace VRCOSC.Game.Packages;

public class PackageSource
{
    private readonly HttpClient httpClient = new();

    private readonly PackageManager packageManager;
    private readonly string repoOwner;
    private readonly string repoName;
    public readonly PackageType PackageType;

    private Repository? repository;
    private Release? latestRelease;
    private PackageFile? packageFile;

    public string InternalReference => $"{repoOwner}#{repoName}";

    public PackageSourceState State { get; private set; } = PackageSourceState.Unknown;
    public string? PackageID => packageFile?.PackageID;
    public string? LatestVersion => latestRelease?.TagName;

    public Action<LoadingInfo>? Progress;

    public bool IsInstalled() => packageManager.IsInstalled(this);
    public bool IsUpdateAvailable() => false;
    public string GetInstalledVersion() => string.Empty;

    public bool IsIncompatible() => State is PackageSourceState.InvalidPackageFile or PackageSourceState.SDKIncompatible;
    public bool IsUnavailable() => State is PackageSourceState.MissingRepo or PackageSourceState.MissingLatestRelease or PackageSourceState.Unknown;
    public bool IsAvailable() => State is PackageSourceState.Valid;

    public PackageSource(PackageManager packageManager, string repoOwner, string repoName, PackageType packageType)
    {
        this.packageManager = packageManager;
        this.repoOwner = repoOwner;
        this.repoName = repoName;
        PackageType = packageType;
    }

    public async Task Refresh(bool forceRemoteGrab)
    {
        Logger.Log($"Checking {InternalReference}");

        if (State is PackageSourceState.Unknown || forceRemoteGrab)
        {
            State = PackageSourceState.Unknown;

            await loadRepository(false);
            await loadLatestRelease(false);
            await loadPackageFile(false);
            checkSDKCompatibility();

            if (State is PackageSourceState.Unknown) State = PackageSourceState.Valid;
        }

        Logger.Log($"{InternalReference} resulted in {State}");
    }

    public async Task Install()
    {
        Progress?.Invoke(new LoadingInfo($"Installing {GetDisplayName()}", 0f, false));
        await packageManager.InstallPackage(this);
        Progress?.Invoke(new LoadingInfo($"Installing {GetDisplayName()}", 1f, true));
    }

    public void Uninstall()
    {
        Progress?.Invoke(new LoadingInfo($"Uninstalling {GetDisplayName()}", 1f, true));
    }

    public List<ReleaseAsset> GetAssets() => latestRelease!.Assets.Where(releaseAsset => packageFile!.Files.Contains(releaseAsset.Name)).ToList();
    public string GetDisplayName() => packageFile?.DisplayName ?? repoName;
    public string GetSourceURL() => repository?.HtmlUrl ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
    public string GetAuthor() => repoOwner;
    public string GetDescription() => repository?.Description ?? "How did you find this";
    public string GetCoverURL() => packageFile?.CoverImageUrl ?? "https://wallpapercave.com/wp/Zs1bPI9.jpg";

    private static SemVersion getCurrentSDKVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();
        return new SemVersion(version.Major, version.Minor, version.Build);
    }

    private async Task loadRepository(bool forceRemoteGrab)
    {
        try
        {
            if (repository is null || forceRemoteGrab)
            {
                repository = null;
                repository = await PackageManager.GITHUB_CLIENT.Repository.Get(repoOwner, repoName);
            }
        }
        catch (ApiException)
        {
            State = PackageSourceState.MissingRepo;
        }
    }

    private async Task loadLatestRelease(bool forceRemoteGrab)
    {
        if (State is PackageSourceState.MissingRepo) return;

        Debug.Assert(repository is not null);

        try
        {
            if (latestRelease is null || forceRemoteGrab)
            {
                latestRelease = null;
                latestRelease = await PackageManager.GITHUB_CLIENT.Repository.Release.GetLatest(repoOwner, repoName);
            }
        }
        catch (ApiException)
        {
            State = PackageSourceState.MissingLatestRelease;
        }
    }

    private async Task loadPackageFile(bool forceRemoteGrab)
    {
        if (State is PackageSourceState.MissingRepo or PackageSourceState.MissingLatestRelease) return;

        Debug.Assert(repository is not null);
        Debug.Assert(latestRelease is not null);

        if (packageFile is null || forceRemoteGrab)
        {
            packageFile = null;

            var packageFileAsset = latestRelease.Assets.SingleOrDefault(asset => asset.Name == "vrcosc.json");

            if (packageFileAsset is null)
            {
                State = PackageSourceState.InvalidPackageFile;
                return;
            }

            var packageFileContents = await (await httpClient.GetAsync(packageFileAsset.BrowserDownloadUrl)).Content.ReadAsStringAsync();

            try
            {
                packageFile = JsonConvert.DeserializeObject<PackageFile>(packageFileContents);
            }
            catch (JsonException)
            {
                State = PackageSourceState.InvalidPackageFile;
                return;
            }

            if (packageFile is null || string.IsNullOrEmpty(packageFile.PackageID))
            {
                State = PackageSourceState.InvalidPackageFile;
            }
        }
    }

    private void checkSDKCompatibility()
    {
        if (State is PackageSourceState.MissingRepo or PackageSourceState.MissingLatestRelease or PackageSourceState.InvalidPackageFile) return;

        var currentSDKVersion = getCurrentSDKVersion();
        var remoteModuleVersion = SemVersionRange.Parse(packageFile!.SDKVersionRange, SemVersionRangeOptions.Loose);

        if (!currentSDKVersion.Satisfies(remoteModuleVersion))
        {
            State = PackageSourceState.SDKIncompatible;
        }
    }
}
