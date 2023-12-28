// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;
using VRCOSC.Screens.Loading;

namespace VRCOSC.Screens.Main.Repo;

public partial class RepoTabHeader : Container
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

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
                        TextColour = Colours.WHITE0,
                        Action = async () =>
                        {
                            LoadingScreen.Title.Value = "Refreshing listing";
                            LoadingScreen.Description.Value = "Sit tight. We're gathering the latest data";

                            var refreshAction = appManager.PackageManager.RefreshAllSources(true);
                            LoadingScreen.SetAction(refreshAction);
                            refreshAction.OnComplete += () => game.OnListingRefresh?.Invoke();
                            await refreshAction.Execute();
                        }
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
                        Enabled = { Value = true }
                    }
                }
            },
            new FillFlowContainer
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new RepoTabHeaderFilter
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = 285
                    }
                }
            }
        };

        Refresh();
    }

    public void Refresh()
    {
        updateAllButton.Alpha = appManager.PackageManager.Sources.Any(remoteModuleSource => remoteModuleSource.IsUpdateAvailable()) ? 1 : 0;
    }
}
