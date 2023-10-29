// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageList : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer listingFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 0,
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
                            Direction = FillDirection.Vertical,
                            Masking = true,
                            CornerRadius = 5
                        }
                    }
                }
            }
        };

        populate();
    }

    public void Refresh()
    {
        populate();
    }

    private void populate()
    {
        listingFlow.Clear();

        listingFlow.Add(new ModulePackageListHeader
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 50
        });

        var even = false;
        appManager.RemoteModuleSourceManager.Sources.ForEach(remoteModuleSource =>
        {
            listingFlow.Add(new ModulePackageInstance(remoteModuleSource, even));
            even = !even;
        });

        listingFlow.Add(new Box
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 5,
            Colour = Colours.GRAY0
        });
    }
}
