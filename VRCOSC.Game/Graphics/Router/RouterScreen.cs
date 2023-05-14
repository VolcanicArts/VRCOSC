// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterScreen : BaseScreen
{
    [Resolved]
    private RouterManager routerManager { get; set; } = null!;

    private FillFlowContainer<RouterDataFlowEntry> routerDataFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
    }

    protected override BaseHeader CreateHeader() => new RouterHeader();

    protected override Drawable CreateBody() => new BasicScrollContainer
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        ClampExtension = 0,
        ScrollbarVisible = false,
        ScrollContent =
        {
            Child = routerDataFlow = new FillFlowContainer<RouterDataFlowEntry>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Direction = FillDirection.Full,
                LayoutEasing = Easing.OutQuad,
                LayoutDuration = 150
            }
        }
    };

    protected override void LoadComplete()
    {
        var drawableRouterDataSpawner = new DrawableRouterDataSpawner();
        routerDataFlow.Add(drawableRouterDataSpawner);
        routerDataFlow.SetLayoutPosition(drawableRouterDataSpawner, 1);
        routerDataFlow.ChangeChildDepth(drawableRouterDataSpawner, float.MinValue);

        routerManager.Store.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (RouterData newRouterData in e.NewItems)
                {
                    var drawableRouterData = new DrawableRouterData(newRouterData);
                    drawableRouterData.Position = routerDataFlow[^1].Position;
                    routerDataFlow.Add(drawableRouterData);
                }
            }
        }, true);
    }
}
