// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Screens.Main.Home;
using VRCOSC.Game.Screens.Main.Modules;
using VRCOSC.Game.Screens.Main.Repo;
using VRCOSC.Game.Screens.Main.Run;
using VRCOSC.Game.Screens.Main.Settings;

namespace VRCOSC.Game.Screens.Main.Tabs;

public partial class TabBar : Container
{
    public static readonly IReadOnlyDictionary<Tab, TabDefinition> TABS = new Dictionary<Tab, TabDefinition>
    {
        { Tab.Home, new TabDefinition(FontAwesome.Solid.Home, typeof(HomeTab)) },
        { Tab.Repo, new TabDefinition(FontAwesome.Solid.Download, typeof(RepoTab)) },
        { Tab.Modules, new TabDefinition(FontAwesome.Solid.List, typeof(ModulesTab)) },
        { Tab.Run, new TabDefinition(FontAwesome.Solid.Play, typeof(RunTab)) },
        { Tab.Settings, new TabDefinition(FontAwesome.Solid.Cog, typeof(SettingsTab)) }
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

        TABS.ForEach(tabInstance =>
        {
            drawableTabFlow.Add(new DrawableTab
            {
                Tab = tabInstance.Key,
                Icon = tabInstance.Value.Icon
            });
        });
    }
}

public record TabDefinition(IconUsage Icon, Type InstanceType);
