// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.VRChatAPI;

namespace VRCOSC.App.UI.Views.Settings;

public partial class SettingsView
{
    public SettingsView()
    {
        InitializeComponent();

        AppManager.GetInstance().VRChatAPIClient.AuthHandler.State.Subscribe(newState =>
        {
            StateDisplay.Text = newState.ToString();
            TokenDisplay.Text = newState == AuthenticationState.LoggedIn ? AppManager.GetInstance().VRChatAPIClient.AuthHandler.AuthToken : string.Empty;
        });
    }

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
}

public record DeviceDisplay(string ID, string Name);