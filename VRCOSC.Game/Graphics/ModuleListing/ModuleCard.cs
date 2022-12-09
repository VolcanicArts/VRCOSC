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

        TextFlowContainer metadataTextFlow;

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
                    metadataTextFlow = new TextFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding
                        {
                            Vertical = 5
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
                        Child = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Icon = FontAwesome.Solid.Question,
                            IconPadding = 5,
                            CornerRadius = 5,
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

        metadataTextFlow.AddText(Module.Title, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 25);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        var description = Module.Description;
        if (!string.IsNullOrEmpty(Module.Prefab)) description += $". Pairs with {Module.Prefab}";

        metadataTextFlow.AddParagraph(description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });
    }

    private Colour4 calculateModuleColour()
    {
        return Module.ModuleType switch
        {
            ModuleType.General => Colour4.White.Darken(0.15f),
            ModuleType.Health => Colour4.Red,
            ModuleType.Integrations => Colour4.Yellow.Darken(0.25f),
            ModuleType.Accessibility => Colour4.FromHex(@"66ccff"),
            ModuleType.OpenVR => Colour4.FromHex(@"04144d"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
