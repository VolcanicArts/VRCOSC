// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public class ModuleListingGroupDropdownHeader : Container
{
    private readonly Colour4 backgroundColour = VRCOSCColour.Gray5;
    private readonly Colour4 backgroundColourHovered = VRCOSCColour.Gray7;

    public BindableBool State { get; init; }
    public string Title { get; init; }

    private Box background;

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteIcon dropdownSprite;

        Children = new Drawable[]
        {
            background = new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray5
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 50),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Padding = new MarginPadding(5),
                            Child = dropdownSprite = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Icon = FontAwesome.Solid.ArrowCircleRight,
                            },
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = Title,
                            Font = FrameworkFont.Regular.With(size: 30)
                        }
                    }
                }
            }
        };

        State.BindValueChanged(e => dropdownSprite.RotateTo(e.NewValue ? 90 : 0, 400, Easing.OutQuart), true);
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(backgroundColourHovered, 100, Easing.OutQuart);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        background.FadeColour(backgroundColour, 100, Easing.OutQuart);
    }
}
