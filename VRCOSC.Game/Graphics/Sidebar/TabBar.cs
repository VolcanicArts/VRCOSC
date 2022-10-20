// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace VRCOSC.Game.Graphics.Sidebar;

public sealed class TabBar : Container
{
    private const int tab_height = 80;

    private static readonly IReadOnlyDictionary<Tabs, IconUsage> tab_lookup = new Dictionary<Tabs, IconUsage>
    {
        { Tabs.Modules, FontAwesome.Solid.ListUl },
        { Tabs.Settings, FontAwesome.Solid.Cog },
        { Tabs.About, FontAwesome.Solid.Info }
    };

    [Resolved]
    private Bindable<Tabs> selectedTab { get; set; } = null!;

    private readonly SelectionIndicator selectedIndicator;
    private readonly FillFlowContainer<Tab> tabsFlow;

    public TabBar()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3
            },
            selectedIndicator = new SelectionIndicator
            {
                RelativeSizeAxes = Axes.X,
                Height = tab_height
            },
            tabsFlow = new FillFlowContainer<Tab>
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        tab_lookup.ForEach(pair =>
        {
            var (tabName, icon) = pair;
            tabsFlow.Add(new Tab
            {
                RelativeSizeAxes = Axes.X,
                Height = tab_height,
                AssociatedTab = tabName,
                Icon = icon
            });
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        selectedTab.BindValueChanged(id => selectedIndicator.MoveToY(tabsFlow[(int)id.NewValue].Position.Y, 100, Easing.InOutCubic), true);
    }

    private sealed class SelectionIndicator : CircularContainer
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new CircularContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.X,
                X = 0.025f,
                Size = new Vector2(0.05f, 0.8f),
                Masking = true,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White.Opacity(0.5f)
                }
            };
        }
    }
}
