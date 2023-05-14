// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Screen;

public abstract partial class BaseScreen : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                CreateHeader()
                            },
                            null,
                            new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 10,
                                    BorderThickness = 2,
                                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Colour = ThemeManager.Current[ThemeAttribute.Dark],
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        new Container
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding(2),
                                            Child = CreateBody()
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected abstract BaseHeader? CreateHeader();
    protected abstract Drawable? CreateBody();
}
