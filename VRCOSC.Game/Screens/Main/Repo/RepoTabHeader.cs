// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class RepoTabHeader : Container
{
    private TextButton updateAllButton = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new RepoTabHeaderSearchBar
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 570
                    },
                    new TextButton
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 200,
                        BackgroundColour = Colours.Blue0,
                        TextContent = "Refresh",
                        TextFont = FrameworkFont.Regular.With(size: 20),
                        TextColour = Colours.White0
                    },
                    updateAllButton = new TextButton
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 200,
                        BackgroundColour = Colours.Blue0,
                        TextContent = "Update All",
                        TextFont = FrameworkFont.Regular.With(size: 20),
                        TextColour = Colours.White0
                    }
                }
            }
        };
    }
}
