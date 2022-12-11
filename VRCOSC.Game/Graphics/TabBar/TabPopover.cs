// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.TabBar;

public sealed partial class TabPopover : Popover
{
    public TabPopover()
    {
        Background.Colour = ThemeManager.Current[ThemeAttribute.Dark];
        Content.Padding = new MarginPadding(10);
        RelativePositionAxes = Axes.X;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Body.Masking = true;
        Body.CornerRadius = 5;
    }

    protected override Drawable CreateArrow() => new EquilateralTriangle
    {
        Colour = ThemeManager.Current[ThemeAttribute.Dark],
        Origin = Anchor.TopCentre,
        Scale = new Vector2(1.05f)
    };

    protected override void AnchorUpdated(Anchor anchor)
    {
        base.AnchorUpdated(anchor);

        bool isCenteredAnchor = anchor.HasFlagFast(Anchor.x1) || anchor.HasFlagFast(Anchor.y1);
        Body.Margin = new MarginPadding(isCenteredAnchor ? 10 : 3);
        Arrow.Size = new Vector2(isCenteredAnchor ? 12 : 15);
    }

    protected override void PopIn()
    {
        this.FadeIn(200, Easing.OutQuart);
        this.MoveToX(0, 200, Easing.OutQuart);
    }

    protected override void PopOut()
    {
        this.FadeOut(200, Easing.OutQuart);
        this.MoveToX(0.25f, 200, Easing.OutQuart);
    }
}
