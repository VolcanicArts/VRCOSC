// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK.Input;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

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
