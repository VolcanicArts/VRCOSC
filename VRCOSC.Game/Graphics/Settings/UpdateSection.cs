// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class UpdateSection : SectionContainer
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    protected override string Title => "Update";

    protected override void GenerateItems()
    {
        AddDropdown("Update Mode", "How should VRCOSC handle updating?", ConfigManager.GetBindable<UpdateMode>(VRCOSCSetting.UpdateMode));
        AddButton("Check For Update", ThemeManager.Current[ThemeAttribute.Mid], () => game.UpdateManager.CheckForUpdate(configManager.Get<string>(VRCOSCSetting.UpdateRepo)));
    }
}
