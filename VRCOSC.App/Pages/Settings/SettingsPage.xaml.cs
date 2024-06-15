// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Audio;
using VRCOSC.App.ProfileSettings;
using VRCOSC.App.VRChatAPI;

namespace VRCOSC.App.Pages.Settings;

public partial class SettingsPage
{
    private readonly AudioEndpointNotificationClient notificationClient = new();
    private List<DeviceDisplay> devices;

    public SettingsPage()
    {
        InitializeComponent();

        AppManager.GetInstance().VRChatAPIClient.AuthHandler.State.Subscribe(newState =>
        {
            StateDisplay.Text = newState.ToString();

            if (newState == AuthenticationState.LoggedIn)
            {
                TokenDisplay.Text = AppManager.GetInstance().VRChatAPIClient.AuthHandler.AuthToken;
            }
            else
            {
                TokenDisplay.Text = string.Empty;
            }
        }, true);

        ProfileSettingsManager.GetInstance().ChosenInputDeviceID.Subscribe(_ => updateDeviceListAndSelection());
        notificationClient.DeviceListChanged += updateDeviceListAndSelection;

        AudioHelper.RegisterCallbackClient(notificationClient);

        updateDeviceListAndSelection();
    }

    private void updateDeviceListAndSelection() => Dispatcher.Invoke(() =>
    {
        DeviceComboBox.ItemsSource = devices = AudioHelper.GetAllInputDevices().Select(mmDevice => new DeviceDisplay(mmDevice.ID, mmDevice.DeviceFriendlyName)).ToList();
        DeviceComboBox.SelectedItem = devices.Single(device => device.ID == ProfileSettingsManager.GetInstance().ChosenInputDeviceID.Value);
    });

    private void Login_OnClick(object sender, RoutedEventArgs e)
    {
        var username = Username.Text;
        var password = Password.Password;
        AppManager.GetInstance().VRChatAPIClient.AuthHandler.LoginWithCredentials(username, password);
    }

    private void Token_OnClick(object sender, RoutedEventArgs e)
    {
        var token = TokenInput.Text;
        AppManager.GetInstance().VRChatAPIClient.AuthHandler.LoginWithAuthToken(token);
    }

    private void TwoFactorAuth_OnClick(object sender, RoutedEventArgs e)
    {
        var code = TwoFactorAuth.Text;
        AppManager.GetInstance().VRChatAPIClient.AuthHandler.Verify2FACode(code, false);
    }

    private void DevicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;
        var deviceId = (string)comboBox.SelectedValue;

        ProfileSettingsManager.GetInstance().ChosenInputDeviceID.Value = deviceId;
    }
}

public record DeviceDisplay(string ID, string Name);
