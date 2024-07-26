using System;
using VRCOSC.App.ChatBox;

namespace VRCOSC.App.Pages.ChatBox;

public partial class ChatBoxManualInputWindow
{
    public ChatBoxManualInputWindow()
    {
        InitializeComponent();

        ChatBoxManager.GetInstance().IsManualTextOpen = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        ChatBoxManager.GetInstance().IsManualTextOpen = false;
    }

    // have option to choose between send while writing and send button
}

