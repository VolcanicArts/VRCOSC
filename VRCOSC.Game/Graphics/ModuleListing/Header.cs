// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed partial class Header : Container
{
    public Header()
    {
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding(5);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new TypeFilter()
        };
    }
}
