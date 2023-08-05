// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Startup;

public partial class StartupScreen : BaseScreen
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer<StartupDataFlowEntry> startupDataFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
    }

    protected override BaseHeader CreateHeader() => new StartupHeader();

    protected override Drawable CreateBody() => new BasicScrollContainer
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        ClampExtension = 0,
        ScrollbarVisible = false,
        ScrollContent =
        {
            Child = startupDataFlow = new FillFlowContainer<StartupDataFlowEntry>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Direction = FillDirection.Vertical,
                LayoutEasing = Easing.OutQuad,
                LayoutDuration = 150
            }
        }
    };

    protected override void LoadComplete()
    {
        var drawableStartupDataSpawner = new DrawableStartupDataSpawner();
        startupDataFlow.Add(drawableStartupDataSpawner);
        startupDataFlow.SetLayoutPosition(drawableStartupDataSpawner, 1);
        startupDataFlow.ChangeChildDepth(drawableStartupDataSpawner, float.MinValue);

        appManager.StartupManager.Instances.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is not null)
            {
                foreach (StartupInstance newFilePath in e.NewItems)
                {
                    var drawableStartupData = new DrawableStartupData(newFilePath);
                    drawableStartupData.Position = startupDataFlow[^1].Position;
                    startupDataFlow.Add(drawableStartupData);
                }
            }
        }, true);
    }
}
