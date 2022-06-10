// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;

public class ParameterEntry : Container
{
    public Enum Key { get; init; }
    public Bindable<string> Value { get; } = new();

    private Box background;
    private SpriteText text;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            background = new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Invisible
            },
            text = new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Font = FrameworkFont.Regular.With(size: 20),
                Colour = VRCOSCColour.Gray8
            }
        };

        Value.BindValueChanged(e =>
        {
            text.Text = $@"{Key}: {e.NewValue}";
            flashColour();
        }, true);
    }

    private void flashColour()
    {
        background.FlashColour(VRCOSCColour.GrayD, 500, Easing.OutCubic);
    }
}
