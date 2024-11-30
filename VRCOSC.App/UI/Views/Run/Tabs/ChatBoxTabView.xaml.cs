// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.ChatBox;
using VRCOSC.App.UI.Windows.ChatBox;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Run.Tabs;

public partial class ChatBoxTabView
{
    public ChatBoxTabView()
    {
        InitializeComponent();
    }

    private void PopoutChatBox_OnClick(object sender, RoutedEventArgs e)
    {
        var previewWindow = new ChatBoxPreviewWindow();
        WPFUtils.PositionWindow(previewWindow, this, ScreenChoice.SameAsParent, HorizontalPosition.Center, VerticalPosition.Center);
        previewWindow.Show();
    }

    private void LiveTextTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ChatBoxManager.GetInstance().LiveText = LiveTextTextBox.Text;
    }

    private void LiveTextEraser_OnClick(object sender, RoutedEventArgs e)
    {
        LiveTextTextBox.Text = string.Empty;
        LiveTextTextBox.Focus();
    }

    private void ChatBoxTabView_OnLoaded(object sender, RoutedEventArgs e)
    {
        AppManager.GetInstance().State.Subscribe(newState =>
        {
            if (newState == AppManagerState.Stopped) LiveTextTextBox.Text = string.Empty;
        }, true);
    }
}