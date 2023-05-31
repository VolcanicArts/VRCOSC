// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI.Button;

public sealed partial class ToggleButton : IconButton
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Stateful = true;
        IconStateOn = FontAwesome.Solid.Check;
        IconStateOff = null;
        IconPadding = 5;
        BackgroundColour = ThemeManager.Current[ThemeAttribute.Mid];
    }
}
