// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Notifications;

public sealed partial class NotificationContainer : VisibilityContainer
{
    private const int transition_time = 1000;

    protected override bool ShouldBeAlive => true;

    protected override FillFlowContainer Content { get; }

    public NotificationContainer()
    {
        Anchor = Anchor.CentreRight;
        Origin = Anchor.CentreRight;
        RelativePositionAxes = Axes.X;
        RelativeSizeAxes = Axes.Y;
        Width = 250;
        X = 1;
        Masking = true;
        EdgeEffect = new EdgeEffectParameters
        {
            Colour = Colour4.Black.Opacity(0.6f),
            Radius = 5f,
            Type = EdgeEffectType.Shadow
        };

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 15),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new CloseButton
                        {
                            Action = () => this.ForEach(notification => notification.Hide())
                        },
                        Content = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical
                        }
                    }
                }
            }
        };
    }

    protected override void UpdateAfterChildren()
    {
        if (Count == 0) Hide();
    }

    public void Notify(Notification notification)
    {
        Show();
        notification.Show();
        Add(notification);
    }

    protected override void PopIn() => this.MoveToX(0, transition_time, Easing.OutQuad);
    protected override void PopOut() => this.MoveToX(1, transition_time, Easing.InQuad);

    private sealed partial class CloseButton : VRCOSCButton
    {
        private readonly Box background;

        public CloseButton()
        {
            RelativeSizeAxes = Axes.Both;
            ShouldAnimate = false;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray3.Opacity(0.75f)
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(2),
                    FillMode = FillMode.Fit,
                    Child = new SpriteIcon
                    {
                        RelativeSizeAxes = Axes.Both,
                        Icon = FontAwesome.Solid.Get(0xf101)
                    }
                },
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            background.FadeColour(VRCOSCColour.Gray5, 100, Easing.OutQuad);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            background.FadeColour(VRCOSCColour.Gray3.Opacity(0.75f), 100, Easing.InQuad);
        }
    }
}
