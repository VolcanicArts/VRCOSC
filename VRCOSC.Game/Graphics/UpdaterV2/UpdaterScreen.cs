// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace VRCOSC.Game.Graphics.UpdaterV2;

public class UpdaterScreen : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Children = new Drawable[]
        {
            new LoadingCircle
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(100)
            }
        };
    }
}
