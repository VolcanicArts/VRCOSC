using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.Screens;

namespace VRCOSC.Game;

public class VRCOSCGame : VRCOSCGameBase
{
    [Cached]
    private ScreenManager screenManager = new()
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChild = screenManager;
    }
}
