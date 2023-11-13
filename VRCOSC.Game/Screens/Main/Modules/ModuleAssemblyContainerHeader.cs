// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Modules;

public partial class ModuleAssemblyContainerHeader : Container
{
    private static readonly Colour4 default_colour = Colours.GRAY0;
    private static readonly Colour4 hover_colour = Colours.GRAY5;

    private const int onhover_duration = 250;
    private const int onhoverlost_duration = onhover_duration;

    private readonly string title;
    private readonly bool isLocal;
    private Box background = null!;
    private SpriteIcon collapsedChevron = null!;

    [Resolved]
    private Bindable<bool> collapsed { get; set; } = null!;

    public ModuleAssemblyContainerHeader(string title, bool isLocal)
    {
        this.title = title;
        this.isLocal = isLocal;
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
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = default_colour
            },
            new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                X = 11,
                Spacing = new Vector2(10, 0),
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = title,
                        Font = Fonts.BOLD.With(size: 32),
                        Colour = Colours.WHITE2
                    },
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 90,
                        Padding = new MarginPadding
                        {
                            Vertical = 10
                        },
                        Alpha = isLocal ? 1 : 0,
                        Child = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 5,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colours.BLUE0
                                },
                                new SpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "LOCAL",
                                    Font = Fonts.BOLD.With(size: 25)
                                }
                            }
                        }
                    }
                }
            },

            new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Y,
                Width = 40,
                Padding = new MarginPadding(8),
                Child = collapsedChevron = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.Solid.ChevronDown,
                    Shadow = true,
                    ShadowOffset = new Vector2(0, 1),
                    Colour = Colours.WHITE2
                }
            }
        };

        collapsed.BindValueChanged(onCollapseChanged);
    }

    private void onCollapseChanged(ValueChangedEvent<bool> e)
    {
        collapsedChevron.RotateTo(e.NewValue ? 90 : 0, 150, Easing.OutQuart);
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(hover_colour, onhover_duration, Easing.OutQuart);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        background.FadeColour(default_colour, onhoverlost_duration, Easing.OutQuart);
    }

    protected override bool OnClick(ClickEvent e)
    {
        collapsed.Value = !collapsed.Value;
        return true;
    }
}
