// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Settings;

public partial class ThemeSection : SectionContainer
{
    protected override void GenerateItems()
    {
        AddDropdown("Theme", "Select a theme and restart to see the effect", ConfigManager.GetBindable<ColourTheme>(VRCOSCSetting.Theme));
    }
}
