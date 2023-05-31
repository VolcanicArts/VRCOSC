// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace VRCOSC.Game.Graphics.RepoListing;

public partial class RepoListingPopover : PopoverScreen
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new RepoListingScreen
        {
            RelativeSizeAxes = Axes.Both
        };
    }
}
