using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Module;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

public class ModuleScreen : Screen
{
    public Module SourceModule { get; init; }

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
            Children = new Drawable[]
            {
                new ModuleContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    SourceModule = SourceModule
                },
                new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Size = new Vector2(100),
                    Padding = new MarginPadding(10),
                    Child = new IconButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Icon = FontAwesome.Solid.ArrowCircleLeft,
                        Action = this.Exit
                    }
                }
            }
        };
    }

    public override void OnEntering(IScreen last)
    {
        this.MoveToY(1).Then().MoveToY(0, 1000, Easing.OutQuint);
    }

    public override bool OnExiting(IScreen next)
    {
        this.MoveToY(1, 1000, Easing.InQuint);
        return false;
    }
}
