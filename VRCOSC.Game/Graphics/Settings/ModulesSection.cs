// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.ModuleListing;

namespace VRCOSC.Game.Graphics.Settings;

public class ModulesSection : SectionContainer
{
    private ToggleButton autoStartStop = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    protected override string Title => "Modules";

    protected override void GenerateItems()
    {
        Add("Auto Start/Stop", autoStartStop = GenerateToggle());
    }

    protected override void Load()
    {
        autoStartStop.State.Value = configManager.Get<bool>(VRCOSCSetting.AutoStartStop);
    }

    protected override void Save()
    {
        configManager.SetValue(VRCOSCSetting.AutoStartStop, autoStartStop.State.Value);
    }
}
