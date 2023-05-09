// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Run;

public partial class RunScreenMain : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Relative, 0.25f),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable?[]
                {
                    new TerminalContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = 3,
                        Masking = true,
                        CornerRadius = 10,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border]
                    },
                    null,
                    new ParameterContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        BorderThickness = 3,
                        Masking = true,
                        CornerRadius = 10,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border]
                    }
                }
            }
        };
    }
}
