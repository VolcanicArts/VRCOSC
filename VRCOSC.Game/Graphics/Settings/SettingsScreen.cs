// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Settings;

public sealed partial class SettingsScreen : Container
{
    public SettingsScreen()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                ClampExtension = 0,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new TextFlowContainer(t =>
                        {
                            t.Font = FrameworkFont.Regular.With(size: 80);
                            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
                        })
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Text = "Settings"
                        },
                        new FillFlowContainer<SectionContainer>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding(10),
                            Spacing = new Vector2(0, 20),
                            Children = new SectionContainer[]
                            {
                                new ThemeSection(),
                                new OscSection(),
                                new ModulesSection(),
                                new UpdateSection()
                            }
                        }
                    }
                }
            }
        };
    }
}
