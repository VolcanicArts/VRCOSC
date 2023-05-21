// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK.Input;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

[Cached]
public partial class TimelineLayer : Container<DrawableClip>
{
    [Resolved]
    private TimelineEditor timelineEditor { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    public readonly int Priority;

    public TimelineLayer(int priority)
    {
        Priority = priority;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;
    }

    public void Add(Clip clip) => Schedule(() =>
    {
        Add(new DrawableClip(clip));
    });

    public void Remove(Clip clip) => Schedule(() =>
    {
        Children.ForEach(child =>
        {
            if (child.Clip == clip) Schedule(child.RemoveAndDisposeImmediately);
        });
    });

    public (int, int) GetBoundsNearestTo(int value, bool end, bool isCreating = false)
    {
        value = Math.Clamp(value, 0, chatBoxManager.TimelineLengthSeconds);

        var boundsList = new List<int>();

        Children.ForEach(child =>
        {
            var clip = child.Clip;

            if (end)
            {
                if (clip.End.Value != value) boundsList.Add(clip.End.Value);
                boundsList.Add(clip.Start.Value);
            }
            else
            {
                if (clip.Start.Value != value) boundsList.Add(clip.Start.Value);
                boundsList.Add(clip.End.Value);
            }
        });

        boundsList.Add(0);
        boundsList.Add(chatBoxManager.TimelineLengthSeconds);
        boundsList.Sort();

        var lowerBound = boundsList.Last(bound => bound <= value);
        var upperBound = boundsList.First(bound => bound >= value && (!isCreating || bound > lowerBound));

        return (lowerBound, upperBound);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button == MouseButton.Right)
        {
            e.Target = this;
            var xPos = (int)MathF.Floor(e.MousePosition.X / DrawWidth * chatBoxManager.TimelineLengthSeconds);
            timelineEditor.ShowLayerMenu(e, xPos, this);
            return true;
        }

        return base.OnMouseDown(e);
    }
}
