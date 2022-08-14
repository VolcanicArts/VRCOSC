// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace VRCOSC.Game.Graphics.Notifications;

public class NotificationContainer : VisibilityContainer
{
    public override bool AcceptsFocus => true;
    public override bool RequestsFocus => State.Value == Visibility.Visible;

    private FillFlowContainer notificationFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.CentreRight;
        Origin = Anchor.CentreRight;
        RelativePositionAxes = Axes.X;
        RelativeSizeAxes = Axes.Y;
        Width = 200;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            notificationFlow = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                AutoSizeEasing = Easing.InOutQuad,
                AutoSizeDuration = 250
            }
        };
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

    protected override void OnFocusLost(FocusLostEvent e)
    {
        Hide();
        notificationFlow.Clear();
    }
}
