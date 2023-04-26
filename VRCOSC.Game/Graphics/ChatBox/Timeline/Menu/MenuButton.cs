// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline.Menu;

public partial class MenuButton : TextButton
{
    public MenuButton()
    {
        Stateful = false;
        BackgroundColour = ThemeManager.Current[ThemeAttribute.Light];
    }
}
