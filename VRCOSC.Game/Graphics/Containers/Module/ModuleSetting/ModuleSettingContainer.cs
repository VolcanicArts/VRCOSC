using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingContainer : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 100;
        Masking = true;
        CornerRadius = 10;
    }
}
