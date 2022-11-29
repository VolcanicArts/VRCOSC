// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Graphics.Notifications;

public partial class TimedNotification : BasicNotification
{
    public double Delay { get; init; }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Scheduler.AddDelayed(Hide, Delay);
    }
}
