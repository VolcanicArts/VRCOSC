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
    private VRCOSCGame game { get; set; } = null!;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Resolved]
    private RepoTab repoTab { get; set; } = null!;

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
                            game.LoadingScreen.Title.Value = "Refreshing listing";
                            game.LoadingScreen.Description.Value = "Sit tight. We're gathering the latest data";

                            appManager.PackageManager.Progress = loadingInfo =>
                            {
                                game.LoadingScreen.Action.Value = loadingInfo.Action;
                                game.LoadingScreen.Progress.Value = loadingInfo.Progress;

                                if (loadingInfo.Complete)
                                {
                                    game.LoadingScreen.Hide();
                                    repoTab.RefreshListings();
                                }
                            };

                            game.LoadingScreen.Show();
                            await appManager.PackageManager.RefreshAllSources();
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
                        TextColour = Colours.WHITE0
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
