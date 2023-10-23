// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK.Graphics;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class RepoTabHeaderSearchBar : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Masking = true;
        CornerRadius = 5;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY2
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 40),
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
                            Padding = new MarginPadding(10),
                            Child = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Icon = FontAwesome.Solid.Search,
                                Colour = Colours.WHITE1,
                                Rotation = 180
                            }
                        },
                        new SearchBarTextBox
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            }
        };
    }

    private partial class SearchBarTextBox : BasicTextBox
    {
        protected override Color4 SelectionColour => Colours.GRAY3;
        protected override Color4 InputErrorColour => Colours.RED1;

        public SearchBarTextBox()
        {
            BackgroundUnfocused = Colours.GRAY2;
            BackgroundFocused = Colours.GRAY2;
            BackgroundCommit = Colours.Transparent;
            PlaceholderText = "Search";
        }

        protected override Drawable GetDrawableCharacter(char c) => new FallingDownContainer
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            AutoSizeAxes = Axes.Both,
            Child = new SpriteText
            {
                Text = c.ToString(),
                Font = Fonts.REGULAR.With(size: CalculatedTextSize)
            }
        };

        protected override SpriteText CreatePlaceholder() => new FadingPlaceholderText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            X = CaretWidth,
            Colour = Colours.WHITE1,
            Font = Fonts.REGULAR.With(size: 40)
        };
    }
}
