// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace VRCOSC.Game.Graphics.Sidebar;

public sealed class Sidebar : Container
{
    private const int tab_height = 80;

    private FillFlowContainer<Tab> tabsFlow = null!;
    private Container selectedIndicator = null!;

    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    public Sidebar()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
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
                Height = tab_height
            },
            tabsFlow = new FillFlowContainer<Tab>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    new Tab()
                    {
                        Height = tab_height,
                        AssociatedTab = Tabs.Modules,
                        Icon = FontAwesome.Solid.ListUl
                    },
                    new Tab()
                    {
                        Height = tab_height,
                        AssociatedTab = Tabs.Settings,
                        Icon = FontAwesome.Solid.Cog
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        game.SelectedTab.BindValueChanged(id => selectTab(id.NewValue), true);
    }

    private void selectTab(Tabs tab)
    {
        selectedIndicator.MoveToY(tabsFlow[(int)tab].Position.Y, 100, Easing.InOutCubic);
    }

    private sealed class SelectionIndicator : Container
    {
        public SelectionIndicator()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
        }

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
