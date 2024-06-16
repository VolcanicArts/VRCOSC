// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VRCOSC.App.Audio;
using VRCOSC.App.Pages.Settings;
using VRCOSC.App.Settings;
using VRCOSC.App.Themes;
using SpeechEngine = VRCOSC.App.Settings.SpeechEngine;

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

    public bool EnableAppDebug
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.EnableAppDebug).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.EnableAppDebug).Value = value;
    }

    public bool AutoSwitchMicrophone
    {
        get => (bool)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.AutoSwitchMicrophone).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.AutoSwitchMicrophone).Value = value;
    }

    public SpeechEngine SelectedSpeechEngine
    {
        get => (SpeechEngine)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.SelectedSpeechEngine).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.SelectedSpeechEngine).Value = value;
    }

    public string VoskModelDirectory
    {
        get => (string)SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VOSK_ModelDirectory).Value;
        set => SettingsManager.GetInstance().GetObservable(VRCOSCSetting.VOSK_ModelDirectory).Value = value;
    }

    private int selectedPage;

    private List<DeviceDisplay> audioInputDevices;

    public AppSettingsPage()
    {
        InitializeComponent();

        DataContext = this;

        SettingsManager.GetInstance().GetObservable(VRCOSCSetting.SelectedInputDeviceID).Subscribe(_ => updateDeviceListAndSelection(), true);
        SpeechEngineComboBox.ItemsSource = Enum.GetValues<SpeechEngine>();

        setPage(0);
    }

    private void updateDeviceListAndSelection() => Dispatcher.Invoke(() =>
    {
        MicrophoneComboBox.ItemsSource = audioInputDevices = AudioHelper.GetAllInputDevices().Select(mmDevice => new DeviceDisplay(mmDevice.ID, mmDevice.DeviceFriendlyName)).ToList();
        MicrophoneComboBox.SelectedItem = audioInputDevices.Single(device => device.ID == SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedInputDeviceID));
    });

    private void MicrophoneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var deviceId = (string)comboBox.SelectedValue;

        SettingsManager.GetInstance().GetObservable(VRCOSCSetting.SelectedInputDeviceID).Value = deviceId;
    }

    private void SpeechEngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TODO: If more speech engines are added, swap between engine views
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
        SpeechTabButton.Background = pageIndex == 6 ? (Brush)FindResource("CBackground6") : (Brush)FindResource("CBackground3");

        GeneralContainer.Visibility = pageIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        OscContainer.Visibility = pageIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        AutomationContainer.Visibility = pageIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
        UpdatesContainer.Visibility = pageIndex == 3 ? Visibility.Visible : Visibility.Collapsed;
        DeveloperContainer.Visibility = pageIndex == 4 ? Visibility.Visible : Visibility.Collapsed;
        PackagesContainer.Visibility = pageIndex == 5 ? Visibility.Visible : Visibility.Collapsed;
        SpeechContainer.Visibility = pageIndex == 6 ? Visibility.Visible : Visibility.Collapsed;
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

    private void SpeechTabButton_OnClick(object sender, RoutedEventArgs e)
    {
        setPage(6);
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
