// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingContainer : Container
{
    public string Key { get; set; }
    public Modules.Module SourceModule { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 100;
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 3;
        EdgeEffect = VRCOSCEdgeEffects.BasicShadow;
    }
}
