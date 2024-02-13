// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Input;
using Semver;

namespace VRCOSC.Packages;

public enum PackageType
{
    Official,
    Curated,
    Community
}

public class Package
{
    public string ID { get; set; }

    public string Title { get; set; }
    public SemVersion? LatestVersion { get; set; }
    public SemVersion? InstalledVersion { get; set; }
    public PackageType PackageType { get; set; }

    public Package(string id, string title, SemVersion latestVersion, SemVersion? installedVersion, PackageType packageType)
    {
        ID = id;
        Title = title;
        LatestVersion = latestVersion;
        InstalledVersion = installedVersion;
        PackageType = packageType;
    }

    #region UI

    public Visibility UIInstallVisible => InstalledVersion is null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUnInstallVisible => InstalledVersion is not null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UIUpgradeVisible => InstalledVersion is not null && LatestVersion?.ComparePrecedenceTo(InstalledVersion) == 1 ? Visibility.Visible : Visibility.Collapsed;

    public ICommand UIInstallButton => new RelayCommand(_ => OnInstallButtonClick());

    private void OnInstallButtonClick()
    {
        MessageBox.Show("WOOOOOOOO");
    }

    public ICommand UIUnInstallButton => new RelayCommand(_ => OnUnInstallButtonClick());

    private void OnUnInstallButtonClick()
    {
        MessageBox.Show("WOOOOOOOO");
    }

    #endregion
}
