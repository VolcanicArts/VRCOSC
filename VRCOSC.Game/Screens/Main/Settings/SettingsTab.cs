// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Settings;

public partial class SettingsTab : Container
{
    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY1
            },
            new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new SettingsToggle(configManager.GetBindable<bool>(VRCOSCSetting.ModuleLogDebug), "Debug Mode", "Debug mode allows modules to log detailed events for more granularity, but it can produce a big log file"),
                    new SettingsToggle(configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop), "Auto Start/Stop", "Allows VRCOSC to start/stop modules when VRChat opens/closes"),
                    new SettingsToggle(configManager.GetBindable<bool>(VRCOSCSetting.UseLegacyPorts), "Use Legacy Ports", "Prefer the legacy 9000/9001 ports over using OSCQuery")
                }
            }
        };
    }
}
