using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingContainer : Container
{
    public ModuleSetting ModuleSetting { get; init; }

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
