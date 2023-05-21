// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI.Button;

public partial class TextButton : BasicButton
{
    public string Text { get; init; } = string.Empty;
    public float FontSize { get; init; } = 30;

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new SpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Font = FrameworkFont.Regular.With(size: FontSize),
            Colour = ThemeManager.Current[ThemeAttribute.Text],
            Text = Text,
            Shadow = true,
            ShadowColour = ThemeManager.Current[ThemeAttribute.Darker]
        });
    }
}
