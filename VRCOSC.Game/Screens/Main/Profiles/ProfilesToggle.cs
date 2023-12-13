// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;

namespace VRCOSC.Screens.Main.Profiles;

public partial class ProfilesToggle : Container
{
    private readonly Bindable<bool> bindable;
    private readonly string title;
    private readonly string description;

    public ProfilesToggle(Bindable<bool> bindable, string title, string description)
    {
        this.bindable = bindable;
        this.title = title;
        this.description = description;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.X,
                Height = 60,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Text = title,
                        Font = Fonts.BOLD.With(size: 28),
                        Colour = Colours.WHITE0,
                        Y = -5
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = description,
                        Font = Fonts.REGULAR.With(size: 22),
                        Colour = Colours.WHITE2,
                        Y = 2
                    }
                }
            },
            new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Padding = new MarginPadding(10),
                Child = new CheckBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    BackgroundColour = Colours.GRAY6,
                    BorderColour = Colours.GRAY3,
                    Icon = FontAwesome.Solid.Check,
                    IconSize = 28,
                    State = bindable.GetBoundCopy()
                }
            }
        };
    }
}
