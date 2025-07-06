// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NAudio.CoreAudioApi;
using VRCOSC.App.Audio;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class SpeechView
{
    private readonly AudioEndpointNotificationClient audioEndpointNotificationClient = new();

    public SpeechView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        audioEndpointNotificationClient.DeviceChanged += (_, _, _) => onDefaultDeviceChanged();
        audioEndpointNotificationClient.DeviceListChanged += updateInputDeviceList;
    }

    private void onDefaultDeviceChanged()
    {
        var selectedMicrophoneId = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedMicrophoneID);
        if (!string.IsNullOrEmpty(selectedMicrophoneId)) return;

        audioCapture?.StopCapture();
        startAudioCapture(selectedMicrophoneId);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        updateInputDeviceList();

        AudioDeviceHelper.RegisterCallbackClient(audioEndpointNotificationClient);

        SettingsManager.GetInstance().GetObservable<SpeechModel>(VRCOSCSetting.SpeechModel).Subscribe(onSpeechModelUpdate, true);
        SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedMicrophoneID).Subscribe(onSelectedMicrophoneUpdate, true);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        AudioDeviceHelper.UnRegisterCallbackClient(audioEndpointNotificationClient);

        audioCapture?.StopCapture();
        audioCapture = null;

        SettingsManager.GetInstance().GetObservable<SpeechModel>(VRCOSCSetting.SpeechModel).Unsubscribe(onSpeechModelUpdate);
        SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedMicrophoneID).Unsubscribe(onSelectedMicrophoneUpdate);
    }

    private void onSpeechModelUpdate(SpeechModel newModel) => Dispatcher.Invoke(() =>
    {
        ModelCustomTextBox.Visibility = newModel == Settings.SpeechModel.Custom ? Visibility.Visible : Visibility.Collapsed;

        switch (newModel)
        {
            case Settings.SpeechModel.Custom:
                break;

            case Settings.SpeechModel.Tiny:
                if (!AppManager.GetInstance().Storage.Exists("runtime/whisper/ggml-tiny.bin")) AppManager.GetInstance().InstallSpeechModel(Settings.SpeechModel.Tiny);
                break;

            case Settings.SpeechModel.Small:
                if (!AppManager.GetInstance().Storage.Exists("runtime/whisper/ggml-small.bin")) AppManager.GetInstance().InstallSpeechModel(Settings.SpeechModel.Small);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newModel), newModel, null);
        }
    });

    private void onSelectedMicrophoneUpdate(string selectedMicrophoneId) => Dispatcher.BeginInvoke(() =>
    {
        audioCapture?.StopCapture();
        startAudioCapture(selectedMicrophoneId);
    });

    private List<DeviceDisplay> audioInputDevices = [];
    private AudioCapture? audioCapture;

    private void updateInputDeviceList() => Dispatcher.BeginInvoke(() =>
    {
        var selectedMicrophone = SettingsManager.GetInstance().GetValue<string>(VRCOSCSetting.SelectedMicrophoneID);

        var defaultDevice = new DeviceDisplay(string.Empty, "-- Use Default --");
        audioInputDevices = [defaultDevice];
        audioInputDevices.AddRange(AudioDeviceHelper.GetAllInputDevices().Select(mmDevice => new DeviceDisplay(mmDevice.ID, mmDevice.FriendlyName)));

        MicrophoneComboBox.ItemsSource = audioInputDevices;
        MicrophoneComboBox.SelectedItem = audioInputDevices.SingleOrDefault(device => device.ID == selectedMicrophone) ?? defaultDevice;
    });

    private void startAudioCapture(string selectedMicrophoneId)
    {
        var device = string.IsNullOrEmpty(selectedMicrophoneId) ? WasapiCapture.GetDefaultCaptureDevice() : AudioDeviceHelper.GetDeviceByID(selectedMicrophoneId);
        if (device is null) return;

        audioCapture = new AudioCapture(device);
        audioCapture.OnNewDataAvailable += onAudioDataAvailable;
        audioCapture.StartCapture();
    }

    private void onAudioDataAvailable()
    {
        Debug.Assert(audioCapture is not null);

        var data = audioCapture.GetBufferedData();
        audioCapture.ClearBuffer();

        var maxValue = data.Max() * SettingsManager.GetInstance().GetValue<float>(VRCOSCSetting.SpeechMicVolumeAdjustment);

        Dispatcher.BeginInvoke(() => MicrophoneDebugProgressBar.Value = maxValue);
    }

    private void MicrophoneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var deviceId = (string)comboBox.SelectedValue;

        SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SelectedMicrophoneID).Value = deviceId;
    }

    public IEnumerable<SpeechModel> SpeechModelSource => Enum.GetValues<SpeechModel>();

    public Observable<string> SpeechModelPath => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.SpeechModelPath);
    public Observable<bool> SpeechEnabled => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.SpeechEnabled);
    public Observable<bool> SpeechTranslate => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.SpeechTranslate);
    public Observable<float> ActivationThreshold => SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechNoiseCutoff);
    public Observable<SpeechModel> SpeechModel => SettingsManager.GetInstance().GetObservable<SpeechModel>(VRCOSCSetting.SpeechModel);
    public Observable<int> GPUSelect => SettingsManager.GetInstance().GetObservable<int>(VRCOSCSetting.SpeechGPU);

    public int ConfidenceSliderValue
    {
        get => (int)(SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechConfidence).Value * 100f);
        set => SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechConfidence).Value = value / 100f;
    }

    public int ActivationThresholdSliderValue
    {
        get => (int)(SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechNoiseCutoff).Value * 100f);
        set => SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechNoiseCutoff).Value = value / 100f;
    }

    public float VolumeAdjustmentSliderValue
    {
        get => (float)Interpolation.Map(SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechMicVolumeAdjustment).Value, 0, 3, 0, 300);
        set => SettingsManager.GetInstance().GetObservable<float>(VRCOSCSetting.SpeechMicVolumeAdjustment).Value = (float)Interpolation.Map(value, 0, 300, 0, 3);
    }
}

internal class ActivationThresholdPositionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [float start, double parentWidth])
        {
            return start * parentWidth;
        }

        return 0d;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}

internal record DeviceDisplay(string ID, string Name);