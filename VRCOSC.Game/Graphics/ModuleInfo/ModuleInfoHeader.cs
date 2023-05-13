// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleInfo;

public partial class ModuleInfoHeader : Container
{
    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    private TextFlowContainer textFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Children = new Drawable[]
        {
            new GridContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Relative, 0.8f),
                    new Dimension()
                },
                Content = new[]
                {
                    new[]
                    {
                        null,
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
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
                                textFlow = new TextFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding
                                    {
                                        Bottom = 5,
                                        Horizontal = 10
                                    },
                                    TextAnchor = Anchor.TopCentre
                                }
                            }
                        },
                        null
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        infoModule.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            textFlow.Clear();

            textFlow.AddText(e.NewValue.Title, t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 40);
                t.Colour = ThemeManager.Current[ThemeAttribute.Text];
            });

            textFlow.AddParagraph($"{e.NewValue.Description}. Created by {e.NewValue.Author}", t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 20);
                t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
            });
        }, true);
    }
}
