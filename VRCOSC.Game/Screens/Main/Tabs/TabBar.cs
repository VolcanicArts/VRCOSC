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
using VRCOSC.Graphics;
using VRCOSC.Screens.Main.Home;
using VRCOSC.Screens.Main.Modules;
using VRCOSC.Screens.Main.Profiles;
using VRCOSC.Screens.Main.Repo;
using VRCOSC.Screens.Main.Run;
using VRCOSC.Screens.Main.Settings;

namespace VRCOSC.Screens.Main.Tabs;

public partial class TabBar : Container
{
    public static readonly IReadOnlyDictionary<Tab, TabDefinition> TABS = new Dictionary<Tab, TabDefinition>
    {
        { Tab.Home, new TabDefinition(FontAwesome.Solid.Home, typeof(HomeTab), false) },
        { Tab.Repo, new TabDefinition(FontAwesome.Solid.Download, typeof(RepoTab), false) },
        { Tab.Modules, new TabDefinition(FontAwesome.Solid.List, typeof(ModulesTab), false) },
        { Tab.Run, new TabDefinition(FontAwesome.Solid.Play, typeof(RunTab), false) },
        { Tab.Settings, new TabDefinition(FontAwesome.Solid.Cog, typeof(SettingsTab), true) },
        { Tab.Profiles, new TabDefinition(FontAwesome.Solid.User, typeof(ProfilesTab), true) }
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer<DrawableTab> topDrawableTabFlow;
        FillFlowContainer<DrawableTab> bottomDrawableTabFlow;

        AddInternal(new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = Colours.GRAY0
        });

        AddInternal(topDrawableTabFlow = new FillFlowContainer<DrawableTab>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.Both
        });

        AddInternal(bottomDrawableTabFlow = new FillFlowContainer<DrawableTab>
        {
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            RelativeSizeAxes = Axes.Both
        });

        TABS.ForEach(tabInstance =>
        {
            if (tabInstance.Value.PlaceBottom)
            {
                bottomDrawableTabFlow.Add(new DrawableTab
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Tab = tabInstance.Key,
                    Icon = tabInstance.Value.Icon
                });
            }
            else
            {
                topDrawableTabFlow.Add(new DrawableTab
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Tab = tabInstance.Key,
                    Icon = tabInstance.Value.Icon
                });
            }
        });
    }
}

public record TabDefinition(IconUsage Icon, Type InstanceType, bool PlaceBottom);
