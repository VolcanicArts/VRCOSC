// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Tabs;

public partial class TabBar : Container
{
    private static readonly IReadOnlyDictionary<Tab, IconUsage> tabs = new Dictionary<Tab, IconUsage>
    {
        { Tab.Home, FontAwesome.Solid.Home },
        { Tab.Repo, FontAwesome.Solid.Download }
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<DrawableTab> drawableTabFlow;

        AddInternal(new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = Colours.GRAY0
        });

        AddInternal(drawableTabFlow = new FillFlowContainer<DrawableTab>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.Both
        });

        tabs.ForEach(tabInstance =>
        {
            drawableTabFlow.Add(new DrawableTab
            {
                Tab = tabInstance.Key,
                Icon = tabInstance.Value
            });
        });
    }
}
