// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK.Input;
using VRCOSC.Game.ChatBox.Clips;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class TimelineLayer : Container
{
    [Resolved]
    private TimelineEditor timelineEditor { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;

        Children = new Drawable[]
        {
            // new Box
            // {
            //     Colour = ThemeManager.Current[ThemeAttribute.Dark],
            //     RelativeSizeAxes = Axes.Both,
            // }
        };
    }

    public void Add(Clip clip)
    {
        Add(new DrawableClip(clip));
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        if (e.Button == MouseButton.Right)
        {
            timelineEditor.ShowMenu(e);
            return true;
        }

        return base.OnMouseDown(e);
    }
}
