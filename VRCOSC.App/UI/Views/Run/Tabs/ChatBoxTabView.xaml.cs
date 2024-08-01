// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.UI.Windows.ChatBox;

namespace VRCOSC.App.UI.Views.Run.Tabs;

public partial class ChatBoxTabView
{
    public ChatBoxTabView()
    {
        InitializeComponent();
    }

    private void PopoutChatBox_OnClick(object sender, RoutedEventArgs e)
    {
        new ChatBoxPreviewWindow().Show();
    }
}
