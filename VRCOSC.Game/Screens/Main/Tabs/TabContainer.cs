// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Screens.Main.Home;

namespace VRCOSC.Game.Screens.Main.Tabs;

public partial class TabContainer : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new HomeTab());
    }
}
