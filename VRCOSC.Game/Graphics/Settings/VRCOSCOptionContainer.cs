// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Config;

namespace VRCOSC.Game.Graphics.Settings;

public sealed class VRCOSCOptionContainer : FillFlowContainer<VRCOSCOption>
{
    public VRCOSCOptionContainer()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Direction = FillDirection.Vertical;
        Padding = new MarginPadding(5);
        Spacing = new Vector2(0, 5);
    }

    [BackgroundDependencyLoader]
    private void load(VRCOSCConfigManager configManager)
    {
        Children = new VRCOSCOption[]
        {
            new VRCOSCBoolOption
            {
                Label = "Auto Run",
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
        };
    }
}
