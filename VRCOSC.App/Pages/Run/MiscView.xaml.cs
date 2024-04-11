// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.Pages.Run;

public partial class MiscView
{
    public MiscView()
    {
        InitializeComponent();

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

        ChatBoxText.Text = (string)message.ParameterValue;
    });
}
