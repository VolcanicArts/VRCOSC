// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using VRCOSC.App.Utils;
using SpeechEngine = VRCOSC.App.Settings.SpeechEngine;

// ReSharper disable UnusedMember.Global

namespace VRCOSC.App.Pages.AppSettings;

public partial class AppSettingsPage
{
    public bool AllowPreReleasePackages
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AllowPreReleasePackages).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AllowPreReleasePackages).Value = value;
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

    public bool UseCustomEndpoints
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.UseCustomEndpoints).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.UseCustomEndpoints).Value = value;
    }

    public string? OutgoingEndpoint
    {
        get => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.OutgoingEndpoint).Value;
        set => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.OutgoingEndpoint).Value = value;
    }

    public string? IncomingEndpoint
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

    public bool AutoSwitchMicrophone
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AutoSwitchMicrophone).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AutoSwitchMicrophone).Value = value;
    }

    public SpeechEngine SelectedSpeechEngine
    {
        get => SettingsManager.GetInstance().GetObservable<SpeechEngine>(VRCOSCSetting.SelectedSpeechEngine).Value;
        set => SettingsManager.GetInstance().GetObservable<SpeechEngine>(VRCOSCSetting.SelectedSpeechEngine).Value = value;
    }

    public string? WhisperModelFilePath
    {
        get => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.Whisper_ModelPath).Value;
        set => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.Whisper_ModelPath).Value = value;
    }

    public int SpeechConfidenceSliderValue
    {
        get => (int)(SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechConfidence).Value * 100f);
        set => SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechConfidence).Value = value / 100f;
    }

    private int selectedPage;

    private List<DeviceDisplay> audioInputDevices;

    public AppSettingsPage()
    {
        InitializeComponent();

        DataContext = this;

        SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedInputDeviceID).Subscribe(updateDeviceListAndSelection, true);
        SpeechEngineComboBox.ItemsSource = Enum.GetValues<SpeechEngine>();

        setPage(0);
    }

    private void updateDeviceListAndSelection(string? newDeviceId) => Dispatcher.Invoke(() =>
    {
        MicrophoneComboBox.ItemsSource = audioInputDevices = AudioDeviceHelper.GetAllInputDevices().Select(mmDevice => new DeviceDisplay(mmDevice.ID, mmDevice.FriendlyName)).ToList();

        if (newDeviceId is null) return;

        MicrophoneComboBox.SelectedItem = audioInputDevices.SingleOrDefault(device => device.ID == newDeviceId);
    });

    private void MicrophoneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var deviceId = (string)comboBox.SelectedValue;

        SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedInputDeviceID).Value = deviceId;
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

    private void Whisper_OpenModelList_OnClick(object sender, RoutedEventArgs e)
    {
        openUrl("https://huggingface.co/ggerganov/whisper.cpp/tree/main");
    }

    private static void openUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }
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
