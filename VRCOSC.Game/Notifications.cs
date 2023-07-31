// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Logging;
using osu.Framework.Threading;
using VRCOSC.Game.Graphics.Notifications;

namespace VRCOSC.Game;

/// <summary>
/// Allows for pushing notifications from anywhere within the app
/// </summary>
public static class Notifications
{
    public static Scheduler Scheduler = null!;
    public static NotificationContainer NotificationContainer = null!;

    public static void Notify(Notification notification)
    {
        Scheduler.Add(() => NotificationContainer.Notify(notification));
    }

    public static void Notify(Exception e)
    {
        Notify(new ExceptionNotification(e.Message));
        Logger.Error(e, e.Message, LoggingTarget.Runtime, true);
    }
}
