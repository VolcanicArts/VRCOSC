using System;
using System.Windows;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.ChatBox;

public partial class ChatBoxManualInputWindow
{
    public Observable<string> Text { get; } = new(string.Empty);

    public ChatBoxManualInputWindow()
    {
        InitializeComponent();

        ChatBoxManager.GetInstance().IsManualTextOpen = true;
        ChatBoxManager.GetInstance().ClearText();

        DataContext = this;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        ChatBoxManager.GetInstance().IsManualTextOpen = false;
    }

    private void SendButton_OnClick(object sender, RoutedEventArgs e)
    {
        ChatBoxManager.GetInstance().SendText(Text.Value);
        Text.Value = string.Empty;
    }
}