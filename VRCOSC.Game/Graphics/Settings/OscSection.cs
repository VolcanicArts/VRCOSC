// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings;

public class OscSection : SectionContainer
{
    private VRCOSCTextBox ipAddress = null!;
    private VRCOSCTextBox outgoingPort = null!;
    private VRCOSCTextBox incomingPort = null!;

    protected override string Title => "OSC";

    protected override void GenerateItems()
    {
        Add("IP Address", ipAddress = GenerateTextBox());
        Add("Outgoing Port", outgoingPort = GenerateIntTextBox());
        Add("Incoming Port", incomingPort = GenerateIntTextBox());
    }

    protected override void Load()
    {
        ipAddress.Text = ConfigManager.Get<string>(VRCOSCSetting.IPAddress);
        outgoingPort.Text = ConfigManager.Get<int>(VRCOSCSetting.SendPort).ToString();
        incomingPort.Text = ConfigManager.Get<int>(VRCOSCSetting.ReceivePort).ToString();
    }

    protected override void Save()
    {
        ConfigManager.SetValue(VRCOSCSetting.IPAddress, ipAddress.Text);
        ConfigManager.SetValue(VRCOSCSetting.SendPort, int.Parse(outgoingPort.Text));
        ConfigManager.SetValue(VRCOSCSetting.ReceivePort, int.Parse(incomingPort.Text));
    }
}
