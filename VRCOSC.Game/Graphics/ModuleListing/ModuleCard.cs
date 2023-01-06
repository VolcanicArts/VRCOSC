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

        SpriteText description;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Darker]
            },
            new Box
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                Width = 5,
                Colour = calculateModuleColour()
            },
            new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Padding = new MarginPadding
                {
                    Left = 5
                },
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Padding = new MarginPadding(7),
                        Child = new ToggleButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ShouldAnimate = false,
                            State = (BindableBool)Module.Enabled.GetBoundCopy()
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding
                        {
                            Vertical = 4
                        },
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Font = FrameworkFont.Regular.With(size: 25),
                                Colour = ThemeManager.Current[ThemeAttribute.Text],
                                Text = module.Title
                            },
                            description = new SpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Font = FrameworkFont.Regular.With(size: 20),
                                Colour = ThemeManager.Current[ThemeAttribute.Text]
                            }
                        }
                    }
                }
            },
            new FillFlowContainer
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
                Padding = new MarginPadding(7),
                Spacing = new Vector2(7, 0),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Alpha = Module.HasParameters ? 1 : 0.5f,
                        Child = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Icon = FontAwesome.Solid.Question,
                            IconPadding = 5,
                            CornerRadius = 5,
                            Action = () => infoModule.Value = Module,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Light],
                            Enabled = { Value = Module.HasParameters }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Alpha = Module.HasSettings ? 1 : 0.5f,
                        Child = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Icon = FontAwesome.Solid.Get(0xF013),
                            IconPadding = 5,
                            CornerRadius = 5,
                            Action = () => editingModule.Value = Module,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Light],
                            Enabled = { Value = Module.HasSettings }
                        }
                    }
                }
            }
        };
        var descriptionText = Module.Description;
        if (!string.IsNullOrEmpty(Module.Prefab)) descriptionText += $". Pairs with {Module.Prefab}";

        description.Text = descriptionText;
    }

    private Colour4 calculateModuleColour()
    {
        return Module.Type switch
        {
            Module.ModuleType.General => Colour4.White.Darken(0.15f),
            Module.ModuleType.Health => Colour4.Red,
            Module.ModuleType.Integrations => Colour4.Yellow.Darken(0.25f),
            Module.ModuleType.Accessibility => Colour4.FromHex(@"66ccff"),
            Module.ModuleType.OpenVR => Colour4.FromHex(@"04144d"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
