// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.Notifications;

namespace VRCOSC.Game.Tests.Visual;

public class TestNotifications : VRCOSCTestScene
{
    private NotificationContainer notifications = null!;

    [SetUp]
    public void SetUp()
    {
        Add(new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = Colour4.Gray
        });
        Add(notifications = new NotificationContainer());
    }

    [Test]
    public void TestNotificationContainer()
    {
        AddStep("show", () => notifications.Show());
        AddUntilStep("ensure hidden", () => Precision.AlmostEquals(notifications.Position.X, 1));
        AddStep("notify", notifyBasic);
        AddUntilStep("ensure shown", () => Precision.AlmostEquals(notifications.Position.X, 0));
        AddStep("hide", () => notifications.Hide());
        AddUntilStep("ensure hidden", () => Precision.AlmostEquals(notifications.Position.X, 1));
    }

    [Test]
    public void TestBasicNotification()
    {
        AddStep("notify", notifyBasic);
    }

    [Test]
    public void TestProgressNotification()
    {
        ProgressNotification? progress = null;
        AddStep("notify", () => progress = notifyProgress());
        AddSliderStep("set progress", 0f, 1f, 0f, p =>
        {
            if (progress is not null)
                progress.Progress = p;
        });
    }

    private void notifyBasic()
    {
        notifications.Notify(new BasicNotification
        {
            Title = "Basic Title",
            Description = "This is basic",
            Colour = VRCOSCColour.GreenLight,
            Icon = FontAwesome.Solid.Check
        });
    }

    private ProgressNotification notifyProgress()
    {
        var progress = new ProgressNotification
        {
            Title = "Progress Title",
            Description = "This is progressing",
            Colour = VRCOSCColour.YellowDark,
            Icon = FontAwesome.Solid.ExclamationTriangle
        };
        notifications.Notify(progress);
        return progress;
    }
}
