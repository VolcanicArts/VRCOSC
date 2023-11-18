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

    private readonly FillFlowContainer flowWrapper;
    private readonly BasicScrollContainer scrollContainer;
    private readonly ModulePackageListHeader header;

    protected override FillFlowContainer Content { get; }

    public ModulePackageList()
    {
        InternalChild = flowWrapper = new FillFlowContainer
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
                header = new ModulePackageListHeader
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
                        Child = Content = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical
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
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        populate();
    }

    protected override void UpdateAfterChildren()
    {
        if (flowWrapper.DrawHeight >= DrawHeight)
        {
            scrollContainer.AutoSizeAxes = Axes.None;
            scrollContainer.Height = DrawHeight - header.DrawHeight - 5;
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
        Clear();

        var even = false;
        appManager.RemoteModuleSourceManager.Sources.ForEach(remoteModuleSource =>
        {
            Add(new ModulePackageInstance(remoteModuleSource, even));
            even = !even;
        });
    }
}
