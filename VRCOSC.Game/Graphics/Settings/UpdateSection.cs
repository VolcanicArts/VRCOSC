// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings;

public class UpdateSection : SectionContainer
{
    private VRCOSCDropdown<UpdateMode> updateMode = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    protected override string Title => "Modules";

    protected override void GenerateItems()
    {
        Add("Update Mode", updateMode = GenerateDropdown<UpdateMode>());
    }

    protected override void Load()
    {
        updateMode.Current.Value = configManager.Get<UpdateMode>(VRCOSCSetting.UpdateMode);
    }

    protected override void Save()
    {
        configManager.SetValue(VRCOSCSetting.UpdateMode, updateMode.Current.Value);
    }
}
