// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Screens.Main.Tabs;

public partial class TabContainer : Container
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        TabBar.TABS.Values.ForEach(definition => Add((Drawable)Activator.CreateInstance(definition.InstanceType)!));

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
