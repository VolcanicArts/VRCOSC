using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;
using VRCOSC.Game.Graphics.Containers.Screens.TerminalScreen;

namespace VRCOSC.Game;

public class VRCOSCGame : VRCOSCGameBase
{
    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
        {
            new ModuleCardListingContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            },
            new TerminalContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            },
            new ModuleEditContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            }
        };
    }
}
