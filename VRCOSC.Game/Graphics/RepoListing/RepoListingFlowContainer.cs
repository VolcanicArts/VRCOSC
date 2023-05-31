// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.RepoListing;

public partial class RepoListingFlowContainer : Container
{
    private FillFlowContainer repoFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];
        CornerRadius = 10;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(2),
                Child = new VRCOSCScrollContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    ClampExtension = 0,
                    ScrollContent =
                    {
                        Child = repoFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            LayoutDuration = 150,
                            LayoutEasing = Easing.OutQuint,
                            AutoSizeDuration = 150,
                            AutoSizeEasing = Easing.OutQuint,
                            Padding = new MarginPadding(5),
                            Spacing = new Vector2(0, 5)
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        repoFlow.Add(new DrawableRepoListing(new Uri("https://github.com/VolcanicArts/VRCOSC")));
    }
}
