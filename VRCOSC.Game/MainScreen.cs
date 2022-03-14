using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK.Graphics;
using VRCOSC.Game.Graphics.Containers.Module;
using VRCOSC.Game.Graphics.Containers.Terminal;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

public class MainScreen : Screen
{
    [Cached]
    private TerminalContainer terminalContainer = new()
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        Masking = true,
        CornerRadius = 20
    };

    [Cached]
    private ModuleManager moduleManager = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
        {
            moduleManager,
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Gray
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new ModuleContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    SourceModule = new TestModule()
                }
            }
            // new Container
            // {
            //     Anchor = Anchor.Centre,
            //     Origin = Anchor.Centre,
            //     RelativeSizeAxes = Axes.Both,
            //     Padding = new MarginPadding
            //     {
            //         Top = 10,
            //         Bottom = 10 + footer_height,
            //         Left = 10,
            //         Right = 10
            //     },
            //     Child = terminalContainer
            // },
            // new MainScreenFooter
            // {
            //     Anchor = Anchor.BottomCentre,
            //     Origin = Anchor.BottomCentre,
            //     RelativeSizeAxes = Axes.X,
            //     Height = footer_height
            // }
        };
    }

    private const float footer_height = 50;
}
