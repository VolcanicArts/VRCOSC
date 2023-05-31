// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Github;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.RepoListing;

public partial class DrawableRepoListing : Container
{
    [Resolved]
    private GitHubProvider gitHubProvider { get; set; } = null!;

    private readonly Uri repoUrl;

    public DrawableRepoListing(Uri repoUrl)
    {
        this.repoUrl = repoUrl;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];
        CornerRadius = 5;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Light],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(7)
            }
        };
    }

    protected override async void LoadComplete()
    {
        var repoListing = await gitHubProvider.GetLatestReleaseFor(repoUrl);
    }
}
