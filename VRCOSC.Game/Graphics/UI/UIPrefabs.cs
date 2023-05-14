// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.UI;

public static class UIPrefabs
{
    public static IconButton QuestionButton => new()
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        Circular = true,
        IconShadow = true,
        Icon = FontAwesome.Solid.Question,
        BackgroundColour = ThemeManager.Current[ThemeAttribute.Action]
    };
}
