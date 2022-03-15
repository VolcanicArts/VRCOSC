using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace VRCOSC.Game;

public class VRCOSCGame : VRCOSCGameBase
{
    [Cached]
    private ScreenStack screenStack = new() { RelativeSizeAxes = Axes.Both };

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = screenStack;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        screenStack.Push(new MainScreen());
    }
}
