// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class ModulesSection : SectionContainer
{
    protected override string Title => "Modules";

    protected override void GenerateItems()
    {
        AddToggle("Auto Start/Stop", "Auto start/stop modules on VRChat start/stop", ConfigManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop));
        AddToggle("Auto Focus", "Auto focus VRChat on modules start", ConfigManager.GetBindable<bool>(VRCOSCSetting.AutoFocus));
    }
}
