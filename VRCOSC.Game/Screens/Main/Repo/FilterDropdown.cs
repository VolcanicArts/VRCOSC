// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class FilterDropdown : Container
{
    protected override FillFlowContainer Content { get; }

    public FilterDropdown()
    {
        Anchor = Anchor.BottomCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;
        BorderThickness = 3;
        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY2
            },
            Content = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(13),
                Spacing = new Vector2(0, 7)
            }
        };
    }

    protected override bool OnClick(ClickEvent e) => true;
}

public partial class FilterEntry : FillFlowContainer
{
    public FilterEntry(string title)
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Spacing = new Vector2(0, 5);
        Direction = FillDirection.Vertical;

        Add(new Container
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            AutoSizeAxes = Axes.Both,
            Child = new SpriteText
            {
                Font = Fonts.BOLD.With(size: 27),
                Colour = Colours.WHITE2,
                Text = title,
                Y = -5
            }
        });
    }

    public FilterEntry AddFilter(string name, Bindable<bool> filter)
    {
        Add(new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(5, 0),
            Children = new Drawable[]
            {
                new CheckBox
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    State = filter.GetBoundCopy(),
                    BackgroundColour = Colours.GRAY6,
                    BorderColour = Colours.GRAY6
                },
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5),
                    Child = new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = Fonts.REGULAR.With(size: 25),
                        Colour = Colours.WHITE0,
                        Text = name
                    }
                }
            }
        });

        return this;
    }
}
