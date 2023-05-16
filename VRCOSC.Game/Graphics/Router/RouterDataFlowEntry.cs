// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterDataFlowEntry : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        Width = 400;
        Height = 115;
        Padding = new MarginPadding(5);
    }
}
