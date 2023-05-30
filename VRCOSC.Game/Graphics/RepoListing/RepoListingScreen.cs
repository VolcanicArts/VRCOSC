// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.ModuleInfo;
using VRCOSC.Game.Graphics.Screen;

namespace VRCOSC.Game.Graphics.RepoListing;

public partial class RepoListingScreen : BaseScreen
{
    protected override BaseHeader CreateHeader() => new RepoListingHeader();

    protected override Drawable CreateBody() => new Container
    {
        RelativeSizeAxes = Axes.Both,
        Padding = new MarginPadding(10),
        Child = new GridContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(GridSizeMode.Absolute, 10),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new DrawableInfoCard("VRCOSC is not responsible for any files downloaded unless specified otherwise. Exercise caution"),
                },
                null,
                new Drawable[]
                {
                    new RepoListingFlowContainer()
                }
            }
        }
    };
}
