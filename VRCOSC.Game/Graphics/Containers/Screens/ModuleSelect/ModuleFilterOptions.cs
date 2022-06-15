﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Config;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public class ModuleFilterOptions : Container
{
    [Resolved]
    private ModuleSelection moduleSelection { get; set; }

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
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
                new FillFlowContainer<ModuleFilterOption>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        new ModuleFilterOption
                        {
                            Text = "Auto Start/Stop",
                            InitialState = configManager.Get<bool>(VRCOSCSetting.AutoStartStop),
                            OnOptionChange = state => configManager.SetValue(VRCOSCSetting.AutoStartStop, state)
                        }
                    }
                }
            }
        };
    }
}
