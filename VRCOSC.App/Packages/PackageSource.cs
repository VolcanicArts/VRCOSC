// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Octokit;
using Semver;
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
    public PackageLatestRelease? LatestRelease { get; private set; }
    public PackageFile? PackageFile { get; private set; }
    public string InstalledVersion => packageManager.GetInstalledVersion(this);

    public string InternalReference => $"{RepoOwner}#{RepoName}";
    public string URL => $"https://github.com/{RepoOwner}/{RepoName}";
    public string DownloadURL => $"{URL}/releases/download/{LatestRelease!.Version}";
    private string packageFileURL => $"{DownloadURL}/vrcosc.json";

    public PackageSourceState State { get; private set; } = PackageSourceState.Unknown;
    public string? PackageID => PackageFile?.PackageID;
    public string? LatestVersion => LatestRelease?.Version;

    public bool IsInstalled() => packageManager.IsInstalled(this);

    public bool IsUpdateAvailable()
    {
        if (IsUnavailable() || IsIncompatible() || !IsInstalled()) return false;

        var installedVersion = SemVersion.Parse(InstalledVersion, SemVersionStyles.Any);
        var latestVersion = SemVersion.Parse(LatestVersion, SemVersionStyles.Any);
        return installedVersion.ComparePrecedenceTo(latestVersion) < 0;
    }

    public bool IsIncompatible() => State is PackageSourceState.InvalidPackageFile or PackageSourceState.SDKIncompatible;
    public bool IsUnavailable() => State is PackageSourceState.MissingRepo or PackageSourceState.MissingLatestRelease or PackageSourceState.Unknown;
    public bool IsAvailable() => State is PackageSourceState.Valid;

    public PackageSource(PackageManager packageManager, string repoOwner, string repoName, PackageType packageType = PackageType.Community)
    {
        this.packageManager = packageManager;
        RepoOwner = repoOwner;
        RepoName = repoName;
        PackageType = packageType;
    }

    public void InjectCachedData(PackageRepository? repository, PackageLatestRelease? latestRelease, PackageFile? packageFile)
    {
        Repository = repository;
        LatestRelease = latestRelease;
        PackageFile = packageFile;
    }

    public async Task Refresh(bool forceRemoteGrab, bool allowPreRelease)
    {
        Logger.Log($"Checking {InternalReference}");

        State = PackageSourceState.Unknown;

        await loadRepository(forceRemoteGrab);
        await loadLatestRelease(forceRemoteGrab, allowPreRelease);
        await loadPackageFile(forceRemoteGrab);
        checkSDKCompatibility();

        if (State is PackageSourceState.Unknown) State = PackageSourceState.Valid;

        Logger.Log($"{InternalReference} resulted in {State}");
    }

    public List<string> GetAssets() => LatestRelease!.AssetNames.Where(assetName => PackageFile!.Files.Contains(assetName)).ToList();
    public string DisplayName => PackageFile?.DisplayName ?? RepoName;
    public string Author => $"Created by {RepoOwner}";
    public string Description => Repository?.Description ?? "How did you find this";
    public string CoverURL => PackageFile?.CoverImageUrl ?? "https://wallpapercave.com/wp/Zs1bPI9.jpg";

    private static SemVersion getCurrentSDKVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
        return new SemVersion(version.Major, version.Minor, version.Build);
    }

    private async Task loadRepository(bool forceRemoteGrab)
    {
        try
        {
            if (Repository is null || forceRemoteGrab)
            {
                Repository = null;
                Repository = new PackageRepository(await GitHubProxy.Client.Repository.Get(RepoOwner, RepoName));
            }

            if (Repository is null)
            {
                State = PackageSourceState.MissingRepo;
            }
        }
        catch (ApiException)
        {
            State = PackageSourceState.MissingRepo;
        }
    }

    private async Task loadLatestRelease(bool forceRemoteGrab, bool allowPreRelease)
    {
        if (State is PackageSourceState.MissingRepo) return;

        Debug.Assert(Repository is not null);

        try
        {
            if (LatestRelease is null || forceRemoteGrab)
            {
                LatestRelease = null;
                LatestRelease = new PackageLatestRelease(await GitHubProxy.Client.Repository.Release.GetLatest(RepoOwner, RepoName));

                var releases = await GitHubProxy.Client.Repository.Release.GetAll(RepoOwner, RepoName);

                PackageLatestRelease? localLatestRelease = null;

                if (allowPreRelease)
                {
                    var latestPreRelease = releases.MaxBy(release => release.CreatedAt);
                    if (latestPreRelease is not null) localLatestRelease = new PackageLatestRelease(latestPreRelease);
                }

                if (localLatestRelease is null)
                {
                    var latestRelease = releases.Where(release => !release.Prerelease).MaxBy(release => release.CreatedAt);
                    if (latestRelease is not null) localLatestRelease = new PackageLatestRelease(latestRelease);
                }

                LatestRelease = localLatestRelease;
            }

            if (LatestRelease is null)
            {
                State = PackageSourceState.MissingLatestRelease;
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

        Debug.Assert(Repository is not null);
        Debug.Assert(LatestRelease is not null);

        if (PackageFile is null || forceRemoteGrab)
        {
            PackageFile = null;

            if (!LatestRelease.AssetNames.Contains("vrcosc.json"))
            {
                State = PackageSourceState.InvalidPackageFile;
                return;
            }

            var packageFileContents = await (await httpClient.GetAsync(packageFileURL)).Content.ReadAsStringAsync();

            try
            {
                PackageFile = JsonConvert.DeserializeObject<PackageFile>(packageFileContents);
            }
            catch (JsonException)
            {
                State = PackageSourceState.InvalidPackageFile;
                return;
            }

            if (PackageFile is null || string.IsNullOrEmpty(PackageFile.PackageID))
            {
                State = PackageSourceState.InvalidPackageFile;
            }
        }

        if (PackageFile is null)
        {
            State = PackageSourceState.InvalidPackageFile;
        }
    }

    private void checkSDKCompatibility()
    {
        if (State is PackageSourceState.MissingRepo or PackageSourceState.MissingLatestRelease or PackageSourceState.InvalidPackageFile) return;

        try
        {
            var currentSDKVersion = getCurrentSDKVersion();
            var remoteModuleVersion = SemVersionRange.Parse(PackageFile!.SDKVersionRange, SemVersionRangeOptions.Loose);

            if (!currentSDKVersion.Satisfies(remoteModuleVersion))
            {
                State = PackageSourceState.SDKIncompatible;
            }
        }
        catch (Exception)
        {
            State = PackageSourceState.SDKIncompatible;
        }
    }

    #region UI

    public string UILatestVersion
    {
        get
        {
            if (IsUnavailable())
                return "Unavailable";

            if (IsIncompatible())
                return "Incompatible";

            return LatestVersion!;
        }
    }

    public Brush UILatestVersionColour
    {
        get
        {
            if (IsUnavailable())
                return (Brush)Application.Current.FindResource("CRedL");

            if (IsIncompatible())
                return (Brush)Application.Current.FindResource("COrange");

            return (Brush)Application.Current.FindResource("CForeground1");
        }
    }

    public Visibility UIInstallVisible => !IsInstalled() && IsAvailable() && !IsIncompatible() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUnInstallVisible => IsInstalled() ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUpgradeVisible => IsUpdateAvailable() && IsAvailable() && !IsIncompatible() ? Visibility.Visible : Visibility.Collapsed;

    public ICommand UIInstallButton => new RelayCommand(_ => OnInstallButtonClick());
    public ICommand UIUnInstallButton => new RelayCommand(_ => OnUnInstallButtonClick());

    private void OnInstallButtonClick()
    {
        var action = packageManager.InstallPackage(this);
        action.OnComplete += () => AppManager.GetInstance().Refresh(PageLookup.Packages);
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Installing {DisplayName}", action);
    }

    private void OnUnInstallButtonClick()
    {
        var result = MessageBox.Show("Are you sure you want to uninstall this package?\nUninstalling will remove all saved module data, and all ChatBox data containing modules provided by this package.", "Uninstall Warning", MessageBoxButton.YesNo);

        if (result != MessageBoxResult.Yes) return;

        var action = packageManager.UninstallPackage(this);
        action.OnComplete += () => AppManager.GetInstance().Refresh(PageLookup.Packages);
        _ = MainWindow.GetInstance().ShowLoadingOverlay($"Uninstalling {DisplayName}", action);
    }

    #endregion
}
