// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics;

public class LoadingSpriteText : SpriteText
{
    public new Bindable<string> Text = new(string.Empty);
    public Bindable<bool> ShouldAnimate = new(true);

    private int periodCount;

    [BackgroundDependencyLoader]
    private void load()
    {
        Text.BindValueChanged(_ =>
        {
            base.Text = Text.Value;
            periodCount = 0;
            Scheduler.CancelDelayedTasks();
            Scheduler.AddDelayed(updateTextPeriods, 500, true);
        }, true);
    }

    private void updateTextPeriods()
    {
        if (!ShouldAnimate.Value)
        {
            base.Text = Text.Value;
            return;
        }

        periodCount++;
        if (periodCount == 4) periodCount = 0;
        base.Text = Text.Value + new string('.', periodCount);
    }
}
