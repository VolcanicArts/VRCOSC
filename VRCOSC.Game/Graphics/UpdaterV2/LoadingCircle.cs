// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Graphics.UpdaterV2;

public class LoadingCircle : Container
{
    private const float total_inner_circles = 10;
    private const float total_loop_time = 2000;

    [BackgroundDependencyLoader]
    private void load()
    {
        for (var i = 0; i < total_inner_circles; i++)
        {
            Add(new InnerCircle
            {
                Index = i,
                ParentSize = Size.X
            });
        }
    }

    private sealed class InnerCircle : Container
    {
        public int Index { get; init; }
        public float ParentSize { get; init; }

        public InnerCircle()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var sizeMultiplier = ModuleMaths.Map(Index / total_inner_circles, 0, 1, 0.25f, 1);
            var maxSize = ParentSize * 0.15f;

            Child = new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Position = new Vector2(0, (ParentSize / 2) - (maxSize / 2)),
                Size = new Vector2(maxSize * sizeMultiplier),
                Masking = true,
                Child = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White
                }
            };
        }

        protected override void LoadComplete()
        {
            var offset = (Index / total_inner_circles) * 360;
            this.RotateTo(offset).RotateTo(360 + offset, total_loop_time).Loop();
        }
    }
}
