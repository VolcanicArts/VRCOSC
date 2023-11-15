// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageList : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer flowWrapper = null!;
    private BasicScrollContainer scrollContainer = null!;
    private FillFlowContainer listingFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            flowWrapper = new FillFlowContainer
            {
                Name = "Flow Wrapper",
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new ModulePackageListHeader
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 50
                    },
                    scrollContainer = new BasicScrollContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        ClampExtension = 0,
                        ScrollbarVisible = false,
                        ScrollContent =
                        {
                            Children = new Drawable[]
                            {
                                listingFlow = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical
                                }
                            }
                        }
                    },
                    new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        Colour = Colours.GRAY0
                    }
                }
            }
        };

        populate();
    }

    protected override void UpdateAfterChildren()
    {
        if (flowWrapper.DrawHeight >= DrawHeight)
        {
            scrollContainer.AutoSizeAxes = Axes.None;
            scrollContainer.Height = DrawHeight - 55;
        }
        else
        {
            scrollContainer.AutoSizeAxes = Axes.Y;
        }
    }

    public void Refresh()
    {
        populate();
    }

    private void populate()
    {
        listingFlow.Clear();

        var even = false;
        appManager.RemoteModuleSourceManager.Sources.ForEach(remoteModuleSource =>
        {
            listingFlow.Add(new ModulePackageInstance(remoteModuleSource, even));
            even = !even;
        });
    }
}
