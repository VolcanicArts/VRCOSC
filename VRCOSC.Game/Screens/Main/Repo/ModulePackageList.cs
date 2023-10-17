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
    private static readonly int[] column_widths = { 250, 150, 150, 150, 80 };

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer listFlow = null!;

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
                Colour = Colours.Dark
            },
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ScrollContent =
                {
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new ModulePackageListingHeader(column_widths),
                                listFlow = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y
                                }
                            }
                        }
                    }
                }
            }
        };

        for (var index = 0; index < appManager.RemoteModuleSourceManager.Sources.Count; index++)
        {
            var remoteModuleSource = appManager.RemoteModuleSourceManager.Sources[index];
            listFlow.Add(new ModulePackageListing(column_widths, remoteModuleSource, index % 2 == 0));
        }
    }
}
