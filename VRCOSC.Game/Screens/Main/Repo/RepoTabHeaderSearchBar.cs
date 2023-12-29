// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Graphics;

namespace VRCOSC.Screens.Main.Repo;

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
            new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(40),
                        Padding = new MarginPadding(10),
                        Child = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Icon = FontAwesome.Solid.Search,
                            Colour = Colours.WHITE1
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = Width - 40 + 5,
                        Child = new SearchBarTextBox
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            X = -5
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
            BackgroundUnfocused = Colours.Transparent;
            BackgroundFocused = Colours.Transparent;
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
                Font = Fonts.REGULAR.With(size: FontSize)
            }
        };

        protected override SpriteText CreatePlaceholder() => new FadingPlaceholderText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Colour = Colours.WHITE1,
            Font = Fonts.REGULAR.With(size: 40)
        };
    }
}
