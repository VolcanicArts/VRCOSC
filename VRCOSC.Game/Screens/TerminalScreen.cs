using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using VRCOSC.Game.Graphics.Containers.Terminal;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

public class TerminalScreen : Screen
{
    [Resolved]
    private ModuleManager moduleManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativePositionAxes = Axes.Both;

        InternalChild = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding(10),
            Child = new TerminalContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 20
            }
        };
    }

    public override void OnEntering(IScreen last)
    {
        this.MoveToY(1).Then().MoveToY(0, 1000, Easing.OutQuint).Finally((_) => moduleManager.Start());
    }

    public override bool OnExiting(IScreen next)
    {
        this.MoveToY(1, 1000, Easing.InQuint);
        return false;
    }
}
