// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.TabBar;

public sealed partial class TabSelector : Container<DrawableTab>
{
    private static readonly IReadOnlyDictionary<Tab, IconUsage> icon_lookup = new Dictionary<Tab, IconUsage>
    {
        { Tab.Modules, FontAwesome.Solid.ListUl },
        { Tab.ChatBox, FontAwesome.Solid.Get(62074) },
        { Tab.Settings, FontAwesome.Solid.Cog },
        { Tab.Router, FontAwesome.Solid.Get(61920) },
        { Tab.About, FontAwesome.Solid.Info }
    };

    protected override FillFlowContainer<DrawableTab> Content { get; }

    public TabSelector()
    {
        RelativeSizeAxes = Axes.Both;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Dark]
            },
            Content = new FillFlowContainer<DrawableTab>
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical
            }
        };

        Enum.GetValues<Tab>().ForEach(tab =>
        {
            Add(new DrawableTab
            {
                Tab = tab,
                Icon = icon_lookup[tab]
            });
        });
    }
}
