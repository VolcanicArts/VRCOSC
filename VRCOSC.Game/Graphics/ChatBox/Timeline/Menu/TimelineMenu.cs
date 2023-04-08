// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline.Menu;

public partial class TimelineMenu : VisibilityContainer
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private ChatBoxScreen chatBoxScreen { get; set; } = null!;

    private Container innerContainer = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = innerContainer = new Container
        {
            Width = 200,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 300
                        }
                    }
                }
            }
        };
    }

    protected override void PopIn()
    {
        Alpha = 1;

        if (Position.Y + innerContainer.DrawHeight < host.Window.ClientSize.Height)
            innerContainer.Origin = innerContainer.Anchor = Position.X + innerContainer.DrawWidth < host.Window.ClientSize.Width ? Anchor.TopLeft : Anchor.TopRight;
        else
            innerContainer.Origin = innerContainer.Anchor = Position.X + innerContainer.DrawWidth < host.Window.ClientSize.Width ? Anchor.BottomLeft : Anchor.BottomRight;
    }

    protected override void PopOut()
    {
        Alpha = 0;
    }

    public void SetPosition(MouseDownEvent e)
    {
        e.Target = chatBoxScreen;
        Position = e.MousePosition;
    }
}
