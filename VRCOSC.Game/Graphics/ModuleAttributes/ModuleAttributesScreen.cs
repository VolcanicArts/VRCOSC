// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes;

public partial class ModuleAttributesScreen : Container
{
    private ModuleAttributeFlowContainer settingFlow = null!;
    private ModuleAttributeFlowContainer parameterFlow = null!;

    [Resolved(name: "EditingModule")]
    private Bindable<Module?> editingModule { get; set; } = null!;

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
                            new Dimension(GridSizeMode.Absolute, 65),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new ModuleAttributesHeader
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre
                                }
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
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding(10),
                                            Child = new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                ColumnDimensions = new[]
                                                {
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Absolute, 5),
                                                    new Dimension()
                                                },
                                                Content = new[]
                                                {
                                                    new Drawable?[]
                                                    {
                                                        settingFlow = new ModuleAttributeFlowContainer("Setting"),
                                                        null,
                                                        parameterFlow = new ModuleAttributeFlowContainer("Parameter")
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

    protected override void LoadComplete()
    {
        editingModule.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            settingFlow.AttributeFlow.AttributeList.Clear();
            settingFlow.AttributeFlow.AttributeList.AddRange(e.NewValue.Settings.Values);

            parameterFlow.AttributeFlow.AttributeList.Clear();
            parameterFlow.AttributeFlow.AttributeList.AddRange(e.NewValue.Parameters.Values);
        }, true);
    }
}
