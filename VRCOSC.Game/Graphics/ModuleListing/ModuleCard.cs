// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed partial class ModuleCard : Container
{
    public readonly Module Module;

    [Resolved(name: "EditingModule")]
    private Bindable<Module?> editingModule { get; set; } = null!;

    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    public ModuleCard(Module module)
    {
        Module = module;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];

        FillFlowContainer textFlow;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Darker]
            },
            new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 5),
                    new Dimension()
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable?[]
                    {
                        new Box
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.Both,
                            Colour = calculateModuleColour()
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(5, 0),
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fit,
                                            Child = new ToggleButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                ShouldAnimate = false,
                                                State = Module.Enabled.GetBoundCopy()
                                            }
                                        },
                                        textFlow = new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical
                                        }
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.Both,
                                    Spacing = new Vector2(5, 0),
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fit,
                                            Child = new IconButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                Icon = FontAwesome.Solid.Question,
                                                IconPadding = 5,
                                                Action = () => infoModule.Value = Module,
                                                BackgroundColour = ThemeManager.Current[ThemeAttribute.Light]
                                            }
                                        },
                                        new Container
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fit,
                                            Child = new IconButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                Icon = FontAwesome.Solid.Get(0xF013),
                                                IconPadding = 5,
                                                Action = () => editingModule.Value = Module,
                                                BackgroundColour = ThemeManager.Current[ThemeAttribute.Light]
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

        textFlow.Add(new SpriteText
        {
            Text = module.Title,
            Font = FrameworkFont.Regular.With(size: 20),
            Colour = ThemeManager.Current[ThemeAttribute.Text],
        });

        var descriptionText = Module.Description;
        if (!string.IsNullOrEmpty(Module.Prefab)) descriptionText += $". Pairs with {Module.Prefab}";

        textFlow.Add(new SpriteText
        {
            Text = descriptionText,
            Font = FrameworkFont.Regular.With(size: 17),
            Colour = ThemeManager.Current[ThemeAttribute.Text],
        });
    }

    private Colour4 calculateModuleColour()
    {
        return Module.Type switch
        {
            Module.ModuleType.General => Colour4.White.Darken(0.15f),
            Module.ModuleType.Health => Colour4.Red,
            Module.ModuleType.Integrations => Colour4.Yellow.Darken(0.25f),
            Module.ModuleType.OpenVR => Colour4.FromHex(@"04144d"),
            Module.ModuleType.Accessibility => Colour4.FromHex(@"0278D8"),
            Module.ModuleType.NSFW => Colour4.Black.Lighten(0.1f),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
