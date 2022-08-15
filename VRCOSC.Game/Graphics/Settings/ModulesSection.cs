// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.ModuleListing;

namespace VRCOSC.Game.Graphics.Settings;

public class ModulesSection : SectionContainer
{
    private ToggleButton autoStartStop = null!;
    private ToggleButton autoFocus = null!;

    protected override string Title => "Modules";

    protected override void GenerateItems()
    {
        Add("Auto Start/Stop", autoStartStop = GenerateToggle());
        Add("Auto Focus VRChat", autoFocus = GenerateToggle());
    }

    protected override void Load()
    {
        autoStartStop.State.Value = ConfigManager.Get<bool>(VRCOSCSetting.AutoStartStop);
        autoFocus.State.Value = ConfigManager.Get<bool>(VRCOSCSetting.AutoFocus);
    }

    protected override void Save()
    {
        ConfigManager.SetValue(VRCOSCSetting.AutoStartStop, autoStartStop.State.Value);
        ConfigManager.SetValue(VRCOSCSetting.AutoFocus, autoFocus.State.Value);
    }
}
