// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.ModuleListing;

namespace VRCOSC.Game.Graphics.Settings;

public class VRCOSCBoolOption : VRCOSCOption
{
    public Bindable<bool> State { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding(5),
            FillMode = FillMode.Fit,
            Child = new ToggleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 7,
                State = (BindableBool)State,
            },
        });
    }
}
