// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Modules.Remote;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageListing : Container
{
    private readonly int[] columnWidths;
    private readonly RemoteModuleSource remoteModuleSource;
    private readonly bool even;

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
            new GridContainer
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
                            Text = remoteModuleSource.LatestRelease?.TagName ?? "Unavailable"
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
                        null,
                        null
                    }
                }
            }
        };
    }
}
