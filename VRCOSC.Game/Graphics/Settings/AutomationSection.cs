// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class AutomationSection : SectionContainer
{
    protected override string Title => "Automation";

    protected override void GenerateItems()
    {
        AddToggle("Start/Stop with VRChat", "Auto start/stop modules on VRChat start/stop", ConfigManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop));
        AddToggle("Close with SteamVR", "Should VRCOSC close when SteamVR closes?", ConfigManager.GetBindable<bool>(VRCOSCSetting.AutoStopOpenVR));
    }
}
