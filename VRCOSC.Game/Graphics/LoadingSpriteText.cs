// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;

// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.Game.Graphics;

/// <inheritdoc />
/// <summary>
/// SpriteText that allows for a loading animation to be played on the text
/// <para>I.E: Test. Test.. Test... Test</para>
/// </summary>
public class LoadingSpriteText : SpriteText
{
    /// <summary>
    /// The current text
    /// </summary>
    public Bindable<string> CurrentText = new();

    /// <summary>
    /// Whether the loading animation should be playing
    /// <para>If this is set to false in the middle of the animation, it will reset</para>
    /// </summary>
    public BindableBool ShouldAnimate = new(true);

    /// <summary>
    /// The time it takes to go from 0 to 3 periods.
    /// <para>Default is 2 seconds, so 0.5 seconds per period addition</para>
    /// </summary>
    public int LoadLength { get; init; } = 2000;

    /// <summary>
    /// The max number of periods to display before resetting
    /// </summary>
    public int MaxPeriodCount { get; init; } = 3;

    private int periodCount;

    [BackgroundDependencyLoader]
    private void load()
    {
        CurrentText.BindValueChanged(_ => resetAnimation());
        ShouldAnimate.BindValueChanged(e => resetAnimation(1, e.NewValue));
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        resetAnimation();
    }

    private void resetAnimation(int periodReset = 0, bool begin = true)
    {
        periodCount = periodReset;
        Text = CurrentText.Value;

        Scheduler.CancelDelayedTasks();

        if (begin)
        {
            // add once to update immediately
            Scheduler.AddOnce(updateText);
            Scheduler.AddDelayed(updateText, LoadLength / (MaxPeriodCount + 1f), true);
        }
    }

    private void updateText()
    {
        Text = $"{CurrentText.Value}{new string('.', periodCount++)}";
        if (periodCount > MaxPeriodCount) periodCount = 0;
    }
}
