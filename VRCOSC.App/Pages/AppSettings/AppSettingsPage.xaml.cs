// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VRCOSC.App.Settings;
using VRCOSC.App.Themes;

// ReSharper disable UnusedMember.Global

namespace VRCOSC.App.Pages.AppSettings;

public partial class AppSettingsPage
{
    public bool AllowPreReleasePackages
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.AllowPreReleasePackages).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.AllowPreReleasePackages).Value = value;
    }

    public bool VRCAutoStart
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStart).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStart).Value = value;
    }

    public bool VRCAutoStop
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStop).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VRCAutoStop).Value = value;
    }

    public bool OVRAutoOpen
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.OVRAutoOpen).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.OVRAutoOpen).Value = value;
    }

    public bool OVRAutoClose
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.OVRAutoClose).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.OVRAutoClose).Value = value;
    }

    public bool ModuleLogDebug
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ModuleLogDebug).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ModuleLogDebug).Value = value;
    }

    public IEnumerable<Theme> ThemeSource => Enum.GetValues<Theme>();

    public int Theme
    {
        get => (int)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.Theme).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.Theme).Value = value;
    }

    public bool UseCustomEndpoints
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.UseCustomEndpoints).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.UseCustomEndpoints).Value = value;
    }

    public string OutgoingEndpoint
    {
        get => (string)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.OutgoingEndpoint).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.OutgoingEndpoint).Value = value;
    }

    public string IncomingEndpoint
    {
        get => (string)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.IncomingEndpoint).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.IncomingEndpoint).Value = value;
    }

    public bool UseChatBoxBlacklist
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ChatBoxWorldBlacklist).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ChatBoxWorldBlacklist).Value = value;
    }

    public int ChatBoxSendInterval
    {
        get => (int)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ChatBoxSendInterval).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ChatBoxSendInterval).Value = value;
    }

    public bool TrayOnClose
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.TrayOnClose).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.TrayOnClose).Value = value;
    }

    private int selectedPage;

    public AppSettingsPage()
    {
        InitializeComponent();

        DataContext = this;

        setPage(0);
    }

    private void setPage(int pageIndex)
    {
        selectedPage = pageIndex;

        GeneralTabButton.Background = pageIndex == 0 ? (Brush)FindResource("CBackground6") : (Brush)FindResource("CBackground3");
        OscTabButton.Background = pageIndex == 1 ? (Brush)FindResource("CBackground6") : (Brush)FindResource("CBackground3");
        AutomationTabButton.Background = pageIndex == 2 ? (Brush)FindResource("CBackground6") : (Brush)FindResource("CBackground3");
        UpdatesTabButton.Background = pageIndex == 3 ? (Brush)FindResource("CBackground6") : (Brush)FindResource("CBackground3");
        DeveloperTabButton.Background = pageIndex == 4 ? (Brush)FindResource("CBackground6") : (Brush)FindResource("CBackground3");
        PackagesTabButton.Background = pageIndex == 5 ? (Brush)FindResource("CBackground6") : (Brush)FindResource("CBackground3");

        GeneralContainer.Visibility = pageIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        OscContainer.Visibility = pageIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        AutomationContainer.Visibility = pageIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        UpdatesContainer.Visibility = pageIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        DeveloperContainer.Visibility = pageIndex == 4 ? Visibility.Visible : Visibility.Collapsed;
        PackagesContainer.Visibility = pageIndex == 5 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void GeneralTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(0);
    }

    private void OscTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(1);
    }

    private void AutomationTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(2);
    }

    private void UpdatesTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(3);
    }

    private void DeveloperTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(4);
    }

    private void PackagesTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(5);
    }
}

public class IpPortValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var input = value?.ToString() ?? string.Empty;
        return isValidIpPort(input) ? ValidationResult.ValidResult : new ValidationResult(false, "Invalid IP:Port format");
    }

    private bool isValidIpPort(string input)
    {
        const string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?):([0-9]{1,5})$";
        return Regex.IsMatch(input, pattern);
    }
}
