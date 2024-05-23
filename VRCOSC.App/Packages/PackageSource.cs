// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using Octokit;
using Semver;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;
using Application = System.Windows.Application;

namespace VRCOSC.App.Packages;

public class PackageSource
{
    private readonly HttpClient httpClient = new();

    private readonly PackageManager packageManager;
    public string RepoOwner { get; }
    public string RepoName { get; }
    public PackageType PackageType { get; }

    public PackageRepository? Repository { get; private set; }

    public string InstalledVersion => packageManager.GetInstalledVersion(this);
    public string InternalReference => $"{RepoOwner}#{RepoName}";
    public string URL => $"https://github.com/{RepoOwner}/{RepoName}";

    public PackageSourceState State { get; private set; } = PackageSourceState.Unknown;
    public string? PackageID => Repository?.PackageFile?.PackageID;
    public string? LatestVersion => Repository?.Releases.FirstOrDefault()?.Version;
    public List<PackageRelease> FilteredReleases => Repository!.Releases.Where(packageRelease => (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.AllowPreReleasePackages) && packageRelease.IsPrerelease) || !packageRelease.IsPrerelease)!.ToList();
    public PackageRelease LatestRelease => FilteredReleases.First();
    public PackageRelease? InstalledRelease => FilteredReleases.SingleOrDefault(packageRelease => packageRelease.Version == InstalledVersion);

    public bool IsInstalled() => packageManager.IsInstalled(this);

    public bool IsUpdateAvailable()
    {
        if (IsUnavailable() || !IsInstalled()) return false;

        var installedVersion = SemVersion.Parse(InstalledVersion, SemVersionStyles.Any);
        var latestVersion = SemVersion.Parse(LatestVersion, SemVersionStyles.Any);
        return installedVersion.ComparePrecedenceTo(latestVersion) < 0;
    }

    public bool IsUnavailable() => State is PackageSourceState.MissingRepo or PackageSourceState.NoReleases or PackageSourceState.InvalidPackageFile or PackageSourceState.Unknown;
    public bool IsAvailable() => State is PackageSourceState.Valid;

    public PackageSource(PackageManager packageManager, string repoOwner, string repoName, PackageType packageType = PackageType.Community)
    {
        this.packageManager = packageManager;
        RepoOwner = repoOwner;
        RepoName = repoName;
        PackageType = packageType;
    }

    public void InjectCachedData(PackageRepository repository)
    {
        Repository = repository;
    }

    public async Task Refresh(bool forceRemoteGrab)
    {
        Logger.Log($"Checking {InternalReference}");

        State = PackageSourceState.Unknown;

        await loadRepository(forceRemoteGrab);
        await loadPackageFile();
        await cacheAllReleases(forceRemoteGrab);

        if (State is PackageSourceState.Unknown) State = PackageSourceState.Valid;

        Logger.Log($"{InternalReference} resulted in {State}");
    }

    public string DisplayName => Repository?.PackageFile?.DisplayName ?? RepoName;
    public string Author => $"Created by {RepoOwner}";
    public string Description => Repository?.Description ?? "How did you find this";
    public string CoverURL => Repository?.PackageFile?.CoverImageUrl ?? "https://wallpapercave.com/wp/Zs1bPI9.jpg";

    private async Task loadRepository(bool forceRemoteGrab)
    {
        try
        {
            if (!forceRemoteGrab) return;

            Repository = new PackageRepository(await GitHubProxy.Client.Repository.Get(RepoOwner, RepoName));
        }
        catch (ApiException)
        {
            State = PackageSourceState.MissingRepo;
        }
    }

    private async Task loadPackageFile()
    {
        if (State == PackageSourceState.MissingRepo) return;

        Debug.Assert(Repository is not null);

        if (Repository.PackageFile is null)
        {
            try
            {
                var packageFileURL = $"https://raw.githubusercontent.com/{RepoOwner}/{RepoName}/{Repository.DefaultBranch}/vrcosc.json";
                var packageFileContents = await (await httpClient.GetAsync(packageFileURL)).Content.ReadAsStringAsync();
                Repository.PackageFile = JsonConvert.DeserializeObject<PackageFile>(packageFileContents);
            }
            catch (Exception)
            {
                State = PackageSourceState.InvalidPackageFile;
            }
        }

        if (Repository.PackageFile is null || string.IsNullOrEmpty(Repository.PackageFile.PackageID))
        {
            State = PackageSourceState.InvalidPackageFile;
        }
    }

    private async Task cacheAllReleases(bool forceRemoteGrab)
    {
        if (State is PackageSourceState.MissingRepo or PackageSourceState.InvalidPackageFile) return;

        Debug.Assert(Repository is not null);
        Debug.Assert(Repository.PackageFile is not null);

        try
        {
            if (!forceRemoteGrab) return;

            Repository.Releases.Clear();

            var releases = await GitHubProxy.Client.Repository.Release.GetAll(RepoOwner, RepoName);

            var sortedDictionary = new SortedDictionary<SemVersion, PackageRelease>();

            foreach (Release release in releases)
            {
                if (!SemVersion.TryParse(release.TagName, SemVersionStyles.Any, out var version)) continue;

                var packageRelease = new PackageRelease(release);
                sortedDictionary.Add(version, packageRelease);
            }

            Repository.Releases.AddRange(sortedDictionary.Values.Reverse());

            if (!Repository.Releases.Any())
            {
                State = PackageSourceState.NoReleases;
            }
        }
        catch (ApiException)
        {
            State = PackageSourceState.NoReleases;
        }
    }

    #region UI

    public string UILatestVersion
    {
        get
        {
            if (IsUnavailable())
                return "Unavailable";

            return LatestVersion!;
        }
    }

    public Brush UILatestVersionColour
    {
        get
        {
            if (IsUnavailable())
                return (Brush)Application.Current.FindResource("CRedL");

            return (Brush)Application.Current.FindResource("CForeground1");
        }
    }

    public Visibility UIInstallDropdownVisible => !IsUnavailable() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIInstallVisible => !IsInstalled() && IsAvailable() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUnInstallVisible => IsInstalled() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUpgradeVisible => IsUpdateAvailable() && IsAvailable() ? Visibility.Visible : Visibility.Collapsed;

    #endregion
}
