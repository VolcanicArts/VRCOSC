// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Config;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect.SidePanel;

public sealed class VRCOSCOptionContainer : Container
{
    public VRCOSCOptionContainer()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding(5);
    }

    [BackgroundDependencyLoader]
    private void load(VRCOSCConfigManager configManager)
    {
        Child = new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Masking = true,
            CornerRadius = 10,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray4
                },
                new FillFlowContainer<VRCOSCOption>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(5),
                    Spacing = new Vector2(0, 5),
                    Children = new VRCOSCOption[]
                    {
                        new VRCOSCBoolOption
                        {
                            Label = "Auto Start/Stop",
                            State = configManager.GetBindable<bool>(VRCOSCSetting.AutoStartStop),
                        },
                        new VRCOSCStringOption
                        {
                            Label = "OSC IP Address",
                            Text = configManager.GetBindable<string>(VRCOSCSetting.IPAddress)
                        },
                        new VRCOSCIntOption
                        {
                            Label = "OSC Send Port",
                            Value = configManager.GetBindable<int>(VRCOSCSetting.SendPort)
                        },
                        new VRCOSCIntOption
                        {
                            Label = "OSC Receive Port",
                            Value = configManager.GetBindable<int>(VRCOSCSetting.ReceivePort)
                        }
                    }
                }
            }
        };
    }
}
