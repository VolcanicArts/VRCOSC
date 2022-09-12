// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings;

public sealed class UpdateSection : SectionContainer
{
    private VRCOSCDropdown<UpdateMode> updateMode = null!;

    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    protected override string Title => "Update";

    protected override void GenerateItems()
    {
        Add("Update Mode", updateMode = GenerateDropdown<UpdateMode>());
        AddButton("Check For Update", VRCOSCColour.Gray4, () => game.UpdateManager.CheckForUpdate());
    }

    protected override void Load()
    {
        updateMode.Current.Value = ConfigManager.Get<UpdateMode>(VRCOSCSetting.UpdateMode);
    }

    protected override void Save()
    {
        ConfigManager.SetValue(VRCOSCSetting.UpdateMode, updateMode.Current.Value);
    }
}
