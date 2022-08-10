// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed class ParameterEntry : Container
{
    public string Key { get; init; } = null!;
    public Bindable<string> Value { get; } = new();

    private Box background = null!;
    private SpriteText valueText = null!;

    public ParameterEntry()
    {
        Anchor = Anchor.TopLeft;
        Origin = Anchor.TopLeft;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new FillFlowContainer
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = FrameworkFont.Regular.With(size: 20),
                        Colour = VRCOSCColour.Gray8,
                        Text = $"{Key}: "
                    },
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            background = new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = VRCOSCColour.Invisible
                            },
                            valueText = new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20),
                                Colour = VRCOSCColour.Gray8
                            }
                        }
                    }
                }
            }
        };

        Value.BindValueChanged(e =>
        {
            valueText.Text = e.NewValue;
            flashColour();
        }, true);
    }

    private void flashColour()
    {
        background.FlashColour(VRCOSCColour.GrayD, 500, Easing.OutCubic);
    }
}
