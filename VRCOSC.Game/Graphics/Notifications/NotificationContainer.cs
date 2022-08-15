// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
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

public class NotificationContainer : VisibilityContainer
{
    protected override bool ShouldBeAlive => true;

    private CloseButton closeButton = null!;
    private FillFlowContainer<Notification> notificationFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
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

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
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
                        closeButton = new CloseButton(),
                        notificationFlow = new FillFlowContainer<Notification>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical
                        }
                    }
                }
            }
        };

        closeButton.Action += () => notificationFlow.ForEach(notification => notification.Hide());
    }

    protected override void UpdateAfterChildren()
    {
        if (notificationFlow.Count == 0) Hide();
    }

    public void Notify(Notification notification)
    {
        Show();
        notification.Show();
        notificationFlow.Add(notification);
    }

    protected override void PopIn()
    {
        this.MoveToX(0, 1000, Easing.OutQuad);
    }

    protected override void PopOut()
    {
        this.MoveToX(1, 1000, Easing.InQuad);
    }

    private sealed class CloseButton : VRCOSCButton
    {
        private Box background = null!;

        public CloseButton()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            ShouldAnimate = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
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
