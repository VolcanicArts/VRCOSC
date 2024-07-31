// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Pages.ChatBox;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.Pages.Run.Tabs;

public partial class ChatBoxView
{
    public ChatBoxView()
    {
        InitializeComponent();

        DataContext = this;

        AppManager.GetInstance().State.Subscribe(onAppManagerStateChange, true);
        AppManager.GetInstance().VRChatOscClient.OnParameterSent += OnParameterSent;
    }

    private void onAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
        switch (newState)
        {
            case AppManagerState.Starting:
                ChatBoxText.Text = "";
                break;
        }
    });

    private void OnParameterSent(VRChatOscMessage message) => Dispatcher.Invoke(() =>
    {
        if (!message.IsChatboxInput) return;

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

    private void PopoutChatBox_OnClick(object sender, RoutedEventArgs e)
    {
        new ChatBoxPreviewWindow().Show();
    }
}
