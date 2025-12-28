// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.Settings;
using VRCOSC.App.Updater;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class BehaviourView
{
    public BehaviourView()
    {
        InitializeComponent();
        DataContext = this;
    }

    private readonly Uri connectingHelpUri = new("https://vrcosc.com/docs/v2/connecting");

    public IEnumerable<ConnectionMode> ConnectionModeSource => Enum.GetValues<ConnectionMode>();
    public IEnumerable<UpdateChannel> UpdateChannelSource => Enum.GetValues<UpdateChannel>();

    public Observable<bool> GlobalKeyboardHook => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.GlobalKeyboardHook);
    public Observable<ConnectionMode> ConnectionMode => SettingsManager.GetInstance().GetObservable<ConnectionMode>(VRCOSCSetting.ConnectionMode);
    public Observable<string> OutgoingEndpoint => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.OutgoingEndpoint);
    public Observable<string> IncomingEndpoint => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.IncomingEndpoint);
    public Observable<UpdateChannel> UpdateChannel => SettingsManager.GetInstance().GetObservable<UpdateChannel>(VRCOSCSetting.UpdateChannel);
    public Observable<bool> UseChatBoxWorldBlocklist => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.ChatBoxWorldBlacklist);
    public Observable<int> ChatBoxSendInterval => SettingsManager.GetInstance().GetObservable<int>(VRCOSCSetting.ChatBoxSendInterval);
    public Observable<bool> VRChatAutoStart => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.VRCAutoStart);
    public Observable<bool> VRChatAutoStop => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.VRCAutoStop);
    public Observable<bool> ShowPreReleasePackages => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AllowPreReleasePackages);
    public Observable<bool> AutoUpdatePackages => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AutoUpdatePackages);
    public Observable<string> OSCQueryClientName => SettingsManager.GetInstance().GetObservable<string>(VRCOSCSetting.OSCQueryClientName);

    private void ConnectingHelpButton_OnClick(object sender, RoutedEventArgs e)
    {
        connectingHelpUri.OpenExternally();
    }
}

internal class ConnectionModeVisibilityConverter : IValueConverter
{
    public ConnectionMode VisibleMode { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ConnectionMode mode)
        {
            return mode == VisibleMode ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}