// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ModuleInfo;

public partial class DrawableInfoCard : Container
{
    private readonly string infoString;

    public DrawableInfoCard(string infoString)
    {
        this.infoString = infoString;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];
        CornerRadius = 5;
        Width = 0.75f;
        Masking = true;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 40),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Masking = true,
                                            CornerRadius = 5,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = ColourInfo.GradientVertical(ThemeManager.Current[ThemeAttribute.Pending].Darken(0.25f), ThemeManager.Current[ThemeAttribute.Pending])
                                                },
                                                new SpriteIcon
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Size = new Vector2(VRCOSCGraphicsContants.ONE_OVER_GOLDEN_RATIO),
                                                    Icon = FontAwesome.Solid.ExclamationTriangle,
                                                    Shadow = true
                                                }
                                            }
                                        }
                                    }
                                },
                                null,
                                new TextFlowContainer(t =>
                                {
                                    t.Font = FrameworkFont.Regular.With(size: 20);
                                    t.Colour = ThemeManager.Current[ThemeAttribute.Text];
                                })
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    TextAnchor = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    AutoSizeEasing = Easing.OutQuint,
                                    AutoSizeDuration = 150,
                                    Text = infoString,
                                    Masking = true
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
