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

    public string RepoOwner { get; }
    public string RepoName { get; }
    public PackageType PackageType { get; }

    public PackageRepository? Repository { get; private set; }

    public string InstalledVersion => PackageManager.GetInstance().GetInstalledVersion(this);
    public string InternalReference => $"{RepoOwner}#{RepoName}";
    public string URL => $"https://github.com/{RepoOwner}/{RepoName}";

    public PackageSourceState State { get; private set; } = PackageSourceState.Unknown;
    public string? PackageID => Repository?.PackageFile?.PackageID;
    public string LatestVersion => LatestRelease.Version;
    public PackageRelease LatestRelease => filterReleases(false, true).First();
    public PackageRelease? InstalledRelease => FilteredReleases.SingleOrDefault(packageRelease => packageRelease.Version == InstalledVersion);

    public List<PackageRelease> FilteredReleases => filterReleases(true, true);

    private List<PackageRelease> filterReleases(bool includeInstalledRelease, bool includePreReleases) =>
        Repository!.Releases.Where(packageRelease => SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.AllowPreReleasePackages) && packageRelease.IsPreRelease && includePreReleases
                                                     || !packageRelease.IsPreRelease
                                                     || packageRelease.Version == InstalledVersion && includeInstalledRelease).ToList();

    public bool IsInstalled() => PackageManager.GetInstance().IsInstalled(this);

    public bool IsUpdateAvailable()
    {
        if (IsUnavailable() || !IsInstalled()) return false;

        var installedVersion = SemVersion.Parse(InstalledVersion, SemVersionStyles.Any);
        var latestVersion = SemVersion.Parse(LatestVersion, SemVersionStyles.Any);
        return SemVersion.ComparePrecedence(latestVersion, installedVersion) == 1;
    }

    public PackageRelease? GetLatestPackages(bool includePreRelease)
    {
        var installedIsPreRelease = InstalledRelease?.IsPreRelease ?? false;
        if (installedIsPreRelease && !includePreRelease) return null;

        var latestRelease = filterReleases(true, includePreRelease).FirstOrDefault();
        if (latestRelease is null) return null;

        var installedVersion = SemVersion.Parse(InstalledVersion, SemVersionStyles.Any);
        var latestVersion = SemVersion.Parse(latestRelease.Version, SemVersionStyles.Any);
        return SemVersion.ComparePrecedence(latestVersion, installedVersion) == 1 ? latestRelease : null;
    }

    public bool IsUnavailable() => State is PackageSourceState.MissingRepo or PackageSourceState.NoReleases or PackageSourceState.InvalidPackageFile or PackageSourceState.Unknown;
    public bool IsAvailable() => State is PackageSourceState.Valid;

    public PackageSource(string repoOwner, string repoName, PackageType packageType)
    {
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
        Logger.Log($"Checking {InternalReference}. Force remote grab: {forceRemoteGrab}");

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
            // Need to check the repository and default branch and force a remote grab if they're null because of an older package cache
            if (!forceRemoteGrab && Repository?.DefaultBranch is not null) return;

            Repository = null;
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

        Debug.Assert(Repository?.PackageFile is not null);

        try
        {
            // Always check for releases if there are no releases in the cache even if we're not forcing a remote grab
            // This is also needed for if there is an older package cache
            if (!forceRemoteGrab && Repository.Releases.Count != 0) return;

            var releases = await GitHubProxy.Client.Repository.Release.GetAll(RepoOwner, RepoName);

            var sortedDictionary = new SortedDictionary<SemVersion, PackageRelease>(SemVersion.SortOrderComparer);

            foreach (Release release in releases)
            {
                if (!SemVersion.TryParse(release.TagName, SemVersionStyles.Any, out var version)) continue;

                var packageRelease = new PackageRelease(release);
                sortedDictionary.Add(version, packageRelease);
            }

            Repository.Releases.Clear();
            Repository.Releases.AddRange(sortedDictionary.Values.Reverse());

            if (Repository.Releases.Count == 0)
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

            return LatestVersion;
        }
    }

    public Brush UILatestVersionColour
    {
        get
        {
            if (IsUnavailable()) return (Brush)Application.Current.FindResource("CRedL");

            return LatestRelease.IsPreRelease ? Brushes.DarkOrange : (Brush)Application.Current.FindResource("CForeground1");
        }
    }

    public Visibility UIInstallDropdownVisible => !IsUnavailable() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIInstallVisible => !IsInstalled() && IsAvailable() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUnInstallVisible => IsInstalled() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUpgradeVisible => IsUpdateAvailable() && IsAvailable() ? Visibility.Visible : Visibility.Collapsed;

    #endregion
}