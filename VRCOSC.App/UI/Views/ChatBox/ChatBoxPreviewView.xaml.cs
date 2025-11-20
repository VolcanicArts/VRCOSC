// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows.Data;
using VRCOSC.App.ChatBox;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.ChatBox;

public partial class ChatBoxPreviewView
{
    public Observable<bool> UseMinimalBackground { get; } = new();

    public ChatBoxPreviewView()
    {
        InitializeComponent();

        DataContext = this;

        AppManager.GetInstance().VRChatOscClient.OnVRChatOSCMessageSent += OnVRChatOSCMessageSent;
    }

    private void OnVRChatOSCMessageSent(VRChatOSCMessage message) => Dispatcher.Invoke(() =>
    {
        if (!message.IsChatboxInput) return;

        var currentClip = ChatBoxManager.GetInstance().CurrentClip;
        UseMinimalBackground.Value = (currentClip?.ShouldUseMinimalBackground() ?? false) || (ChatBoxManager.GetInstance().PulseText is not null && ChatBoxManager.GetInstance().PulseMinimalBackground);

        var text = (string)message.ParameterValue;

        if (ChatBoxText.Text != string.Empty && text == string.Empty)
        {
            ChatBoxContainer.FadeOutFromOne(200, () => ChatBoxText.Text = text);
            return;
        }

        if (ChatBoxText.Text == string.Empty && text != string.Empty)
        {
            ChatBoxText.Text = text;
            ChatBoxContainer.FadeInFromZero(200);
            return;
        }

        ChatBoxText.Text = text;
    });
}

public class ChatBoxPreviewBackgroundWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool useMinimalBackground)
        {
            return useMinimalBackground ? 30d : double.NaN;
        }

        return double.NaN;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}