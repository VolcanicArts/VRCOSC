// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI;

public partial class VRCOSCTextBox : BasicTextBox
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    public VRCOSCTextBox()
    {
        BackgroundFocused = ThemeManager.Current[ThemeAttribute.Dark];
        BackgroundUnfocused = ThemeManager.Current[ThemeAttribute.Dark];
        Masking = true;
        CommitOnFocusLost = true;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        host.Window.Resized += KillFocus;
    }

    protected override void KillFocus() => Schedule(base.KillFocus);

    protected override SpriteText CreatePlaceholder()
    {
        return base.CreatePlaceholder().With(t => t.Colour = ThemeManager.Current[ThemeAttribute.Text].Opacity(0.5f));
    }

    protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
    {
        AutoSizeAxes = Axes.Both,
        Child = new SpriteText
        {
            Text = c.ToString(),
            Font = FrameworkFont.Condensed.With(size: CalculatedTextSize),
            Colour = ThemeManager.Current[ThemeAttribute.Text]
        }
    };

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        host.Window.Resized -= KillFocus;
    }
}
