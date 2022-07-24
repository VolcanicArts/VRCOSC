// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed class RunningPopover : Container
{
    private TerminalContainer Terminal;
    private ParameterContainer Parameters;

    [Resolved]
    private ModuleManager moduleManager { get; set; }

    public RunningPopover()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.X;
        X = 1;
        Padding = new MarginPadding(40);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            CornerRadius = 5,
            EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
            Masking = true,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray3
                },
                new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            Terminal = new TerminalContainer(),
                            Parameters = new ParameterContainer()
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Size = new Vector2(60),
                    Padding = new MarginPadding(10),
                    Child = new IconButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = 10,
                        Icon = FontAwesome.Solid.Get(0xf00d),
                        Action = endRun
                    }
                }
            }
        };

        moduleManager.Running.ValueChanged += e =>
        {
            if (e.NewValue)
                this.MoveToX(0, 1000, Easing.OutQuint);
            else
                this.MoveToX(1, 1000, Easing.InQuint).Finally(_ => reset());
        };
    }

    private void endRun()
    {
        _ = moduleManager.Stop();
    }

    private void reset()
    {
        Terminal.ClearTerminal();
        Parameters.ClearParameters();
    }

    protected override bool OnMouseDown(MouseDownEvent e) => true;

    protected override bool OnClick(ClickEvent e) => true;

    protected override bool OnHover(HoverEvent e) => true;
}
