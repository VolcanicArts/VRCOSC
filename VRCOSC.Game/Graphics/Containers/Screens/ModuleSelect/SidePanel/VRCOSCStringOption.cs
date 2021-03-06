// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect.SidePanel;

public class VRCOSCStringOption : VRCOSCOption
{
    public Bindable<string> Text { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            Width = 0.5f,
            Padding = new MarginPadding(5),
            Child = new VRCOSCTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 7,
                Current = Text
            },
        });
    }
}
