// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Screen;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class SettingsScreen : BaseScreen
{
    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
    }

    protected override BaseHeader CreateHeader() => new SettingsHeader();

    protected override Drawable CreateBody() => new Container
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
    };
}
