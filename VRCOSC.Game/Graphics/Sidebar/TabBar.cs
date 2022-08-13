// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace VRCOSC.Game.Graphics.Sidebar;

public class TabBar : Container
{
    protected virtual int TabHeight => 80;

    protected virtual IReadOnlyDictionary<Tabs, IconUsage> TabLookup => new Dictionary<Tabs, IconUsage>()
    {
        { Tabs.Modules, FontAwesome.Solid.ListUl },
        { Tabs.Settings, FontAwesome.Solid.Cog },
        //{ Tabs.About, FontAwesome.Solid.Info }
    };

    [BackgroundDependencyLoader]
    private void load(VRCOSCGame game)
    {
        FillFlowContainer<Tab> tabsFlow;
        Container selectedIndicator;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3
            },
            selectedIndicator = new SelectionIndicator
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                Height = TabHeight
            },
            tabsFlow = new FillFlowContainer<Tab>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical
            }
        };

        TabLookup.ForEach(pair =>
        {
            var (tabName, icon) = pair;
            tabsFlow.Add(new Tab
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                Height = TabHeight,
                AssociatedTab = tabName,
                Icon = icon
            });
        });

        game.SelectedTab.BindValueChanged(id => selectedIndicator.MoveToY(tabsFlow[(int)id.NewValue].Position.Y, 100, Easing.InOutCubic), true);
    }

    private sealed class SelectionIndicator : Container
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
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White.Opacity(0.5f)
                }
            };
        }
    }
}
