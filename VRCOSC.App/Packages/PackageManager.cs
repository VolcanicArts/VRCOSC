// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using VRCOSC.App.Actions;
using VRCOSC.App.Actions.Packages;
using VRCOSC.App.Packages.Serialisation;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Packages;

public class PackageManager : INotifyPropertyChanged
{
    private static PackageManager? instance;
    public static PackageManager GetInstance() => instance ??= new PackageManager(new NativeStorage($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/VRCOSC-V2-WPF"));

    private const string community_tag = "vrcosc-package";

    private readonly Storage storage;
    private readonly SerialisationManager serialisationManager;

    private readonly List<PackageSource> builtinSources = new();

    public ObservableCollection<PackageSource> Sources { get; } = new();
    public readonly Dictionary<string, string> InstalledPackages = new();

    public DateTime CacheExpireTime = DateTime.UnixEpoch;

    public PackageManager(Storage baseStorage)
    {
        storage = baseStorage.GetStorageForDirectory("packages/remote");

        builtinSources.Add(new PackageSource(this, "VolcanicArts", "VRCOSC-Modules", PackageType.Official));
        builtinSources.Add(new PackageSource(this, "DJDavid98", "VRCOSC-BluetoothHeartrate", PackageType.Curated));

        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new PackageManagerSerialiser(baseStorage, this));
    }

    public PackageManager()
    {
    }

    public PackageLoadAction Load()
    {
        builtinSources.ForEach(source => Sources.Add(source));
        serialisationManager.Deserialise();
        return RefreshAllSources(CacheExpireTime + TimeSpan.FromDays(1) <= DateTime.Now);
    }

    public PackageLoadAction RefreshAllSources(bool forceRemoteGrab)
    {
        var packageLoadAction = new PackageLoadAction();

        if (forceRemoteGrab)
        {
            packageLoadAction.AddAction(new DynamicProgressAction("Loading built-in packages", () =>
            {
                Sources.Clear();
                builtinSources.ForEach(source => Sources.Add(source));
            }));
            packageLoadAction.AddAction(loadCommunityPackages());
        }

        packageLoadAction.AddAction(new PackagesRefreshAction(Sources.ToList(), forceRemoteGrab, true));

        packageLoadAction.OnComplete += () =>
        {
            CacheExpireTime = DateTime.Now + TimeSpan.FromDays(1);
            serialisationManager.Serialise();
        };

        return packageLoadAction;
    }

    public PackageInstallAction InstallPackage(PackageSource packageSource)
    {
        var isInstalled = InstalledPackages.ContainsKey(packageSource.PackageID!);
        var installAction = new PackageInstallAction(storage, packageSource, isInstalled);

        installAction.OnComplete += () =>
        {
            InstalledPackages[packageSource.PackageID!] = packageSource.LatestVersion!;
            serialisationManager.Serialise();
            OnPropertyChanged(nameof(UpdateAllButtonVisibility));
            //appManager.ModuleManager.ReloadAllModules();
        };

        return installAction;
    }

    public PackageUninstallAction UninstallPackage(PackageSource packageSource)
    {
        var uninstallAction = new PackageUninstallAction(storage, packageSource);

        uninstallAction.OnComplete += () =>
        {
            InstalledPackages.Remove(packageSource.PackageID!);
            serialisationManager.Serialise();
            //appManager.ModuleManager.ReloadAllModules();
        };

        return uninstallAction;
    }

    public bool IsInstalled(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.ContainsKey(packageSource.PackageID);
    public string GetInstalledVersion(PackageSource packageSource) => packageSource.PackageID is not null && InstalledPackages.TryGetValue(packageSource.PackageID, out var version) ? version : string.Empty;

    private FindCommunityPackagesAction loadCommunityPackages()
    {
        var findCommunityPackages = new FindCommunityPackagesAction();

        var packageSources = new List<PackageSource>();

        var searchProgressAction = new SearchRepositoriesAction(community_tag);
        findCommunityPackages.AddAction(searchProgressAction);

        findCommunityPackages.AddAction(new DynamicProgressAction("Auditing community packages", () =>
        {
            var repos = searchProgressAction.Result!;

            repos.Items.Where(repo => repo.Name != "VRCOSC").ToList().ForEach(repo =>
            {
                var packageSource = new PackageSource(this, repo.Owner.HtmlUrl.Split('/').Last(), repo.Name);
                if (builtinSources.Any(comparedSource => comparedSource.InternalReference == packageSource.InternalReference)) return;

                packageSources.Add(packageSource);
            });
        }));

        return findCommunityPackages;
    }

    #region UI

    private double packageScrollViewerHeight = double.NaN;

    public double PackageScrollViewerHeight
    {
        get => packageScrollViewerHeight;
        set
        {
            packageScrollViewerHeight = value;
            OnPropertyChanged();
        }
    }

    public Visibility UpdateAllButtonVisibility => Sources.Any(packageSource => packageSource.IsUpdateAvailable()) ? Visibility.Visible : Visibility.Collapsed;

    public ICommand UIRefreshButton => new RelayCommand(_ => OnRefreshButtonClick());
    public ICommand UIUpdateAllButton => new RelayCommand(_ => OnUpdateAllButtonClick());

    private async void OnRefreshButtonClick()
    {
        await RefreshAllSources(true).Execute();
        AppManager.GetInstance().Refresh(PageLookup.Packages);
    }

    private async void OnUpdateAllButtonClick()
    {
        // TODO: This probably needs to be its own refresh action
        foreach (var packageSource in Sources.Where(packageSource => packageSource.IsUpdateAvailable()))
        {
            await InstallPackage(packageSource).Execute();
        }

        AppManager.GetInstance().Refresh(PageLookup.Packages);
        OnPropertyChanged(nameof(UpdateAllButtonVisibility));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
