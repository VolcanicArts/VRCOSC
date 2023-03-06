// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class OscSection : SectionContainer
{
    protected override string Title => "OSC";

    protected override void GenerateItems()
    {
        AddTextBox<IPTextBox, string>("IP Address", "The IP address to send and receive OSC values to and from", ConfigManager.GetBindable<string>(VRCOSCSetting.IPAddress));
        AddTextBox<PortTextBox, int>("Outgoing Port", "The port with which to send OSC values to", ConfigManager.GetBindable<int>(VRCOSCSetting.SendPort));
        AddTextBox<PortTextBox, int>("Incoming Port", "The port with which to receive OSC values from", ConfigManager.GetBindable<int>(VRCOSCSetting.ReceivePort));
    }
}
