// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class VRCOSCScrollContainer : VRCOSCScrollContainer<Drawable>
{
    public VRCOSCScrollContainer(Direction scrollDirection = Direction.Vertical)
        : base(scrollDirection)
    {
    }
}

public class VRCOSCScrollContainer<T> : ScrollContainer<T>
    where T : Drawable
{
    protected VRCOSCScrollContainer(Direction scrollDirection = Direction.Vertical)
        : base(scrollDirection)
    {
    }

    protected override void UpdateAfterChildren()
    {
        base.UpdateAfterChildren();

        // we always want this to show
        Scrollbar.Show();
    }

    protected override ScrollbarContainer CreateScrollbar(Direction direction) => new VRCOSCScrollbar(direction);

    private class VRCOSCScrollbar : ScrollbarContainer
    {
        private const float dim_size = 8;

        public VRCOSCScrollbar(Direction direction)
            : base(direction)
        {
            Child = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray2
                }
            };
        }

        public override void ResizeTo(float val, int duration = 0, Easing easing = Easing.None)
        {
            Vector2 size = new Vector2(dim_size)
            {
                [(int)ScrollDirection] = val
            };
            this.ResizeTo(size, duration, easing);
        }
    }
}
