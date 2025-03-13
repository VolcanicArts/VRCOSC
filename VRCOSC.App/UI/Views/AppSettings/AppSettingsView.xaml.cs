// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Themes;
using VRCOSC.App.Updater;
using VRCOSC.App.Utils;

// ReSharper disable UnusedMember.Global

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class AppSettingsView : INotifyPropertyChanged
{
    public bool AllowPreReleasePackages
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AllowPreReleasePackages).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AllowPreReleasePackages).Value = value;
    }

    public bool AutoUpdatePackages
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AutoUpdatePackages).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AutoUpdatePackages).Value = value;
    }

    public bool VRCAutoStart
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.VRCAutoStart).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.VRCAutoStart).Value = value;
    }

    public bool VRCAutoStop
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.VRCAutoStop).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.VRCAutoStop).Value = value;
    }

    public bool OVRAutoOpen
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoOpen).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoOpen).Value = value;
    }

    public bool OVRAutoClose
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoClose).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.OVRAutoClose).Value = value;
    }

    public IEnumerable<Theme> ThemeSource => Enum.GetValues<Theme>();

    public int Theme
    {
        get => (int)SettingsManager.GetInstance().GetObservable<Theme>(VRCOSCSetting.Theme).Value;
        set => SettingsManager.GetInstance().GetObservable<Theme>(VRCOSCSetting.Theme).Value = (Theme)value;
    }

    public IEnumerable<ConnectionMode> OSCModeSource => Enum.GetValues<ConnectionMode>();

    public ConnectionMode ConnectionMode
    {
        get => SettingsManager.GetInstance().GetObservable<ConnectionMode>(VRCOSCSetting.ConnectionMode).Value;
        set => SettingsManager.GetInstance().GetObservable<ConnectionMode>(VRCOSCSetting.ConnectionMode).Value = value;
    }

    public string OutgoingEndpoint
    {
        get => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.OutgoingEndpoint).Value;
        set => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.OutgoingEndpoint).Value = value;
    }

    public string IncomingEndpoint
    {
        get => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.IncomingEndpoint).Value;
        set => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.IncomingEndpoint).Value = value;
    }

    public bool UseChatBoxBlacklist
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.ChatBoxWorldBlacklist).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.ChatBoxWorldBlacklist).Value = value;
    }

    public int ChatBoxSendInterval
    {
        get => SettingsManager.GetInstance().GetObservable<int>(VRCOSCSetting.ChatBoxSendInterval).Value;
        set => SettingsManager.GetInstance().GetObservable<int>(VRCOSCSetting.ChatBoxSendInterval).Value = value;
    }

    public bool StartInTray
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.StartInTray).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.StartInTray).Value = value;
    }

    public bool TrayOnClose
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.TrayOnClose).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.TrayOnClose).Value = value;
    }

    public bool EnableAppDebug
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableAppDebug).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableAppDebug).Value = value;
    }

    public Observable<Visibility> OSCModeCustomVisibility { get; } = new(Visibility.Collapsed);

    public IEnumerable<UpdateChannel> UpdateChannelSource => Enum.GetValues<UpdateChannel>();

    public UpdateChannel SelectedUpdateChannel
    {
        get => SettingsManager.GetInstance().GetObservable<UpdateChannel>(VRCOSCSetting.UpdateChannel).Value;
        set => SettingsManager.GetInstance().GetObservable<UpdateChannel>(VRCOSCSetting.UpdateChannel).Value = value;
    }

    public AppSettingsView()
    {
        InitializeComponent();

        DataContext = this;

        SettingsManager.GetInstance().GetObservable<ConnectionMode>(VRCOSCSetting.ConnectionMode).Subscribe(value => OSCModeCustomVisibility.Value = value == ConnectionMode.Custom ? Visibility.Visible : Visibility.Collapsed, true);

        setPage(0);
    }

    public void FocusAutomationTab()
    {
        AutomationTabButton.IsChecked = true;
        AutomationTabButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private void setPage(int pageIndex)
    {
        GeneralContainer.Visibility = pageIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        OscContainer.Visibility = pageIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        AutomationContainer.Visibility = pageIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        UpdatesContainer.Visibility = pageIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        DebugContainer.Visibility = pageIndex == 4 ? Visibility.Visible : Visibility.Collapsed;
        PackagesContainer.Visibility = pageIndex == 5 ? Visibility.Visible : Visibility.Collapsed;
        SpeechContainer.Visibility = pageIndex == 6 ? Visibility.Visible : Visibility.Collapsed;
        OVRContainer.Visibility = pageIndex == 7 ? Visibility.Visible : Visibility.Collapsed;
        RouterContainer.Visibility = pageIndex == 8 ? Visibility.Visible : Visibility.Collapsed;
        StartupContainer.Visibility = pageIndex == 9 ? Visibility.Visible : Visibility.Collapsed;
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

    private void DebugTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(4);
    }

    private void RouterTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(8);
    }

    private void StartupTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(9);
    }

    private void PackagesTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(5);
    }

    private void SpeechTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(6);
    }

    private void OVRTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(7);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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