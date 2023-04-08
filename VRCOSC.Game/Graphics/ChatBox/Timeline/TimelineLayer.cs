// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK.Input;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

[Cached]
public partial class TimelineLayer : Container<DrawableClip>
{
    [Resolved]
    private TimelineEditor timelineEditor { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;
    }

    public void Add(Clip clip)
    {
        Add(new DrawableClip(clip));
    }

    public void Remove(Clip clip)
    {
        Children.ForEach(child =>
        {
            if (child.Clip == clip) Schedule(child.RemoveAndDisposeImmediately);
        });
    }

    public (int, int) GetBoundsNearestTo(int value, bool end)
    {
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
        boundsList.Add(60);
        boundsList.Sort();

        var lowerBound = boundsList.Last(bound => bound <= value);
        var upperBound = boundsList.First(bound => bound >= value);

        return (lowerBound, upperBound);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button == MouseButton.Right)
        {
            timelineEditor.ShowLayerMenu(e);
            return true;
        }

        return base.OnMouseDown(e);
    }
}
