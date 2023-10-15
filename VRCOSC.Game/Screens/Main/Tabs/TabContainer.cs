// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Screens.Main.Home;
using VRCOSC.Game.Screens.Main.Repo;

namespace VRCOSC.Game.Screens.Main.Tabs;

public partial class TabContainer : Container
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new HomeTab());
        Add(new RepoTab());

        game.SelectedTab.BindValueChanged(e =>
        {
            var id = (int)e.NewValue;

            for (int i = 0; i < Count; i++)
            {
                if (id == i)
                    this[i].Show();
                else
                    this[i].Hide();
            }
        }, true);
    }
}
