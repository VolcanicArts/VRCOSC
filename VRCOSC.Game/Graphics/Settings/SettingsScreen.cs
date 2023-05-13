// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class SettingsScreen : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 65),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new SettingsHeader()
                            },
                            null,
                            new Drawable[]
                            {
                                new Container
                                {
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
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding(10),
                                            Child = new GridContainer
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                RelativeSizeAxes = Axes.Both,
                                                ColumnDimensions = new[]
                                                {
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Absolute, 5),
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Absolute, 5),
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Absolute, 5),
                                                    new Dimension()
                                                },
                                                Content = new[]
                                                {
                                                    new Drawable?[]
                                                    {
                                                        new GeneralSection(),
                                                        null,
                                                        new OscSection(),
                                                        null,
                                                        new AutomationSection(),
                                                        null,
                                                        new UpdateSection()
                                                    }
                                                }
                                            }
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
}
