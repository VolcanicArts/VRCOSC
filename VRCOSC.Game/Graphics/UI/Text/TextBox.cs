// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace VRCOSC.Graphics.UI.Text;

public partial class TextBox : BasicTextBox
{
    public TextBox()
    {
        BackgroundUnfocused = Colours.GRAY2;
        BackgroundFocused = Colours.GRAY2;
        BackgroundCommit = Colours.GRAY2;
        Masking = true;
        CornerRadius = 5;
    }

    protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
    {
        AutoSizeAxes = Axes.Both,
        Child = new SpriteText { Text = c.ToString(), Font = Fonts.REGULAR.With(size: CalculatedTextSize) }
    };

    protected override SpriteText CreatePlaceholder() => base.CreatePlaceholder().With(t => t.Font = Fonts.REGULAR);
}
