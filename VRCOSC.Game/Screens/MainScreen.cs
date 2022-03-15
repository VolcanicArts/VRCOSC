using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Game.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.Module;
using VRCOSC.Game.Graphics.Drawables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

public class MainScreen : Screen
{
    private readonly FillFlowContainer moduleCardFlow;
    private const float footer_height = 50;

    [Resolved]
    private ModuleManager moduleManager { get; set; }

    public MainScreen()
    {
        InternalChildren = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Gray
            },
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 20,
                ScrollbarVisible = false,
                Child = moduleCardFlow = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding
                    {
                        Top = 10,
                        Bottom = 10 + footer_height,
                        Left = 10,
                        Right = 10
                    }
                }
            },
            new MainScreenFooter
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Height = footer_height
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load(ScreenStack screenStack)
    {
        moduleManager.Running.BindValueChanged(e =>
        {
            if (e.NewValue) screenStack.Push(new TerminalScreen());
        });

        moduleManager.Modules.ForEach(pair =>
        {
            var (moduleType, modules) = pair;

            moduleCardFlow.Add(new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 50))
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Text = moduleType.ToString()
            });

            modules.ForEach(module =>
            {
                moduleCardFlow.Add(new ModuleCard
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    SourceModule = module
                });
            });

            moduleCardFlow.Add(new LineSeparator
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(0.95f, 5)
            });
        });
    }
}
