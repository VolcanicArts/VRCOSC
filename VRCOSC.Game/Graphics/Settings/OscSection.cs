// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings;

public class OscSection : SectionContainer
{
    private VRCOSCTextBox ipAddress = null!;
    private VRCOSCTextBox outgoingPort = null!;
    private VRCOSCTextBox incomingPort = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    protected override string Title => "OSC";

    protected override void GenerateItems()
    {
        Add("IP Address", ipAddress = GenerateTextBox());
        Add("Outgoing Port", outgoingPort = GenerateIntTextBox());
        Add("Incoming Port", incomingPort = GenerateIntTextBox());
    }

    protected override void Load()
    {
        ipAddress.Text = configManager.Get<string>(VRCOSCSetting.IPAddress);
        outgoingPort.Text = configManager.Get<int>(VRCOSCSetting.SendPort).ToString();
        incomingPort.Text = configManager.Get<int>(VRCOSCSetting.ReceivePort).ToString();
    }

    protected override void Save()
    {
        configManager.SetValue(VRCOSCSetting.IPAddress, ipAddress.Text);
        configManager.SetValue(VRCOSCSetting.SendPort, int.Parse(outgoingPort.Text));
        configManager.SetValue(VRCOSCSetting.ReceivePort, int.Parse(incomingPort.Text));
    }
}
