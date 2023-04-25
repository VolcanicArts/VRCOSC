// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline.Menu;

public abstract partial class TimelineMenu : VisibilityContainer
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private ChatBoxScreen chatBoxScreen { get; set; } = null!;

    protected override FillFlowContainer Content { get; }

    protected TimelineMenu()
    {
        InternalChild = new Container
        {
            Width = 200,
            AutoSizeAxes = Axes.Y,
            BorderThickness = 2,
            Masking = true,
            CornerRadius = 5,
            EdgeEffect = VRCOSCEdgeEffects.UniformShadow,
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = ThemeManager.Current[ThemeAttribute.Dark].Opacity(0.5f),
                    RelativeSizeAxes = Axes.Both
                },
                Content = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(0, 10)
                }
            }
        };
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(100, Easing.OutQuad);

        if (Position.Y + InternalChild.DrawHeight < host.Window.ClientSize.Height)
            InternalChild.Origin = InternalChild.Anchor = Position.X + InternalChild.DrawWidth < host.Window.ClientSize.Width ? Anchor.TopLeft : Anchor.TopRight;
        else
            InternalChild.Origin = InternalChild.Anchor = Position.X + InternalChild.DrawWidth < host.Window.ClientSize.Width ? Anchor.BottomLeft : Anchor.BottomRight;
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(100, Easing.OutQuad);
    }

    public void SetPosition(MouseDownEvent e)
    {
        e.Target = chatBoxScreen;
        Position = e.MousePosition;
    }
}
