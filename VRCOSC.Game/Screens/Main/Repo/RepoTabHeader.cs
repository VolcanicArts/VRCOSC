// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class RepoTabHeader : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

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
                        BackgroundColour = Colours.BLUE0,
                        TextContent = "Refresh",
                        TextFont = Fonts.REGULAR,
                        TextColour = Colours.WHITE0
                    },
                    updateAllButton = new TextButton
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 200,
                        BackgroundColour = Colours.BLUE0,
                        TextContent = "Update All",
                        TextFont = Fonts.REGULAR,
                        TextColour = Colours.WHITE0,
                        Alpha = appManager.RemoteModuleSourceManager.Sources.Any(remoteModuleSource => remoteModuleSource.IsUpdateAvailable()) ? 1 : 0
                    }
                }
            }
        };
    }
}
