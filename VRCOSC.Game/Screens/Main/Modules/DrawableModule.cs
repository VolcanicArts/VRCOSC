// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.SDK;

namespace VRCOSC.Game.Screens.Main.Modules;

public partial class DrawableModule : Container
{
    [Resolved]
    private ModulesTab modulesTab { get; set; } = null!;

    private readonly Module module;
    private readonly bool even;

    public DrawableModule(Module module, bool even)
    {
        this.module = module;
        this.even = even;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 50;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = even ? Colours.GRAY4 : Colours.GRAY2
            },
            new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new TypeIdentifier(module),
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(36),
                        Child = new CheckBox
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            BackgroundColour = Colours.GRAY6,
                            BorderColour = Colours.GRAY3,
                            Icon = FontAwesome.Solid.Check,
                            State = module.Enabled.GetBoundCopy()
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        Padding = new MarginPadding
                        {
                            Horizontal = 7,
                            Top = 3,
                            Bottom = 5
                        },
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Text = module.Title,
                                Font = Fonts.BOLD.With(size: 25),
                                Colour = Colours.WHITE0
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Text = module.ShortDescription,
                                Font = Fonts.REGULAR.With(size: 20),
                                Colour = Colours.WHITE2
                            }
                        }
                    }
                }
            },
            new FillFlowContainer
            {
                Name = "Button Flow",
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Padding = new MarginPadding(7),
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(7, 0),
                Children = new Drawable[]
                {
                    new DrawableModuleButton
                    {
                        Icon = FontAwesome.Solid.Link,
                        Action = () => modulesTab.ShowParameters(module)
                    },
                    new DrawableModuleButton
                    {
                        Icon = FontAwesome.Solid.Cog,
                        Action = () => modulesTab.ShowSettings(module)
                    }
                }
            }
        };
    }

    private partial class DrawableModuleButton : IconButton
    {
        public DrawableModuleButton()
        {
            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;
            Size = new Vector2(36);
            Masking = true;
            CornerRadius = 5;
            IconSize = 23;
            BackgroundColour = Colours.GRAY6;
        }
    }

    private partial class TypeIdentifier : Container
    {
        private readonly Module module;

        public TypeIdentifier(Module module)
        {
            this.module = module;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            RelativeSizeAxes = Axes.Y;
            Width = 12;
            Padding = new MarginPadding
            {
                Horizontal = 4
            };
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 2,
                RelativeSizeAxes = Axes.X,
                Height = 22,
                Child = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = module.Type.ToColour()
                }
            };
        }
    }
}
