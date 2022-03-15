using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game;

public class MainScreen : Screen
{
    private const float footer_height = 50;

    [Resolved]
    private ModuleManager moduleManager { get; set; }

    public MainScreen()
    {
        InternalChild = new ModuleCardListingContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        };
    }

    [BackgroundDependencyLoader]
    private void load(ScreenStack screenStack)
    {
        moduleManager.Running.BindValueChanged(e =>
        {
            if (e.NewValue) screenStack.Push(new TerminalScreen());
        });
    }
}
