// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageList : Container<ModulePackageInstance>
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Resolved]
    private RepoTab repoTab { get; set; } = null!;

    private readonly FillFlowContainer flowWrapper;
    private readonly BasicScrollContainer scrollContainer;
    private readonly ModulePackageListHeader header;

    protected override FillFlowContainer<ModulePackageInstance> Content { get; }

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
                        Child = Content = new FillFlowContainer<ModulePackageInstance>
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

        repoTab.Filter.BindValueChanged(e => applyFilter(e.NewValue), true);
    }

    private void applyFilter(PackageListingFilter filter)
    {
        var even = false;
        this.Where(modulePackageInstance => modulePackageInstance.Satisfies(filter)).ForEach(modulePackageInstance =>
        {
            modulePackageInstance.Even.Value = even;
            even = !even;
        });
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
        appManager.PackageManager.Sources
                  .OrderByDescending(packageSource => packageSource.IsInstalled())
                  .ThenBy(packageSource => packageSource.PackageType)
                  .ThenBy(packageSource => packageSource.GetDisplayName())
                  .ForEach(packageSource =>
                  {
                      Add(new ModulePackageInstance(packageSource));
                      even = !even;
                  });

        applyFilter(repoTab.Filter.Value);
    }
}
