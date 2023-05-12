// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using osu.Framework.Bindables;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class OscSection : SectionContainer
{
    protected override string Title => "OSC";

    private Bindable<IPEndPoint> sendProxy = new();
    private Bindable<IPEndPoint> receiveProxy = new();

    protected override void GenerateItems()
    {
        sendProxy = new Bindable<IPEndPoint>(new IPEndPoint(IPAddress.Parse(ConfigManager.Get<string>(VRCOSCSetting.SendAddress)), ConfigManager.Get<int>(VRCOSCSetting.SendPort)));
        receiveProxy = new Bindable<IPEndPoint>(new IPEndPoint(IPAddress.Parse(ConfigManager.Get<string>(VRCOSCSetting.ReceiveAddress)), ConfigManager.Get<int>(VRCOSCSetting.ReceivePort)));

        sendProxy.BindValueChanged(e =>
        {
            ConfigManager.SetValue(VRCOSCSetting.SendAddress, e.NewValue.Address.ToString());
            ConfigManager.SetValue(VRCOSCSetting.SendPort, e.NewValue.Port);
        });

        receiveProxy.BindValueChanged(e =>
        {
            ConfigManager.SetValue(VRCOSCSetting.ReceiveAddress, e.NewValue.Address.ToString());
            ConfigManager.SetValue(VRCOSCSetting.ReceivePort, e.NewValue.Port);
        });

        AddTextBox<IPPortTextBox, IPEndPoint>("Outgoing Endpoint", "The IP and port with which to send OSC data to", sendProxy);
        AddTextBox<IPPortTextBox, IPEndPoint>("Incoming Endpoint", "The IP and port with which to receive OSC data from", receiveProxy);
    }
}
