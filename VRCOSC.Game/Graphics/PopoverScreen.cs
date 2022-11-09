// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics;

public abstract class PopoverScreen : VisibilityContainer
{
    private const int transition_time = 500;

    protected override Container<Drawable> Content { get; }

    protected PopoverScreen()
    {
        State.Value = Visibility.Hidden;
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        RelativePositionAxes = Axes.X;
        Padding = new MarginPadding(40);

        InternalChildren = new Drawable[]
        {
            Content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                BorderThickness = 2,
                EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
                BorderColour = VRCOSCColour.Gray0
            },
            new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Size = new Vector2(60),
                Padding = new MarginPadding(10),
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 7,
                    IconPadding = 5,
                    Icon = FontAwesome.Solid.Get(0xf00d),
                    Action = Hide
                }
            }
        };
    }

    protected override void PopIn()
    {
        this.ScaleTo(0.9f).Then().ScaleTo(1.0f, transition_time, Easing.OutQuint);
        this.FadeInFromZero(transition_time, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.ScaleTo(1.0f).Then().ScaleTo(0.9f, transition_time, Easing.OutQuint);
        this.FadeOutFromOne(transition_time, Easing.OutQuint);
    }

    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;
}
