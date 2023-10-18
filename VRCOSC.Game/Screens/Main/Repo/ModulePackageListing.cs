// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Modules.Remote;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageListing : Container
{
    private readonly int[] columnWidths;
    private readonly RemoteModuleSource remoteModuleSource;
    private readonly bool even;

    private LoadingContainer loadingContainer = null!;

    public ModulePackageListing(int[] columnWidths, RemoteModuleSource remoteModuleSource, bool even)
    {
        this.columnWidths = columnWidths;
        this.remoteModuleSource = remoteModuleSource;
        this.even = even;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = even ? Colours.Light : Colours.Mid
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(3),
                Child = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, columnWidths[0]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[1]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[2]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[3]),
                        new Dimension(GridSizeMode.Absolute, columnWidths[4]),
                        new Dimension()
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable?[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20),
                                Text = remoteModuleSource.RepositoryName
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20),
                                Text = remoteModuleSource.LatestRelease?.TagName ?? "Unavailable",
                                Colour = remoteModuleSource.LatestRelease is not null ? Colours.OffWhite : Colours.Red
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20),
                                Text = remoteModuleSource.GetInstalledVersion()
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = FrameworkFont.Regular.With(size: 20),
                                Text = remoteModuleSource.SourceType.ToString()
                            },
                            new BasicButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(20),
                                Action = install
                            },
                            null
                        }
                    }
                }
            },
            loadingContainer = new LoadingContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            }
        };

        loadingContainer.Hide();
    }

    private void install()
    {
        loadingContainer.Show();
    }
}
