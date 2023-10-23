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

public partial class ModulePackageInstance : Container
{
    private readonly RemoteModuleSource remoteModuleSource;
    private readonly bool even;

    public ModulePackageInstance(RemoteModuleSource remoteModuleSource, bool even)
    {
        this.remoteModuleSource = remoteModuleSource;
        this.even = even;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 50;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = even ? Colours.GRAY4 : Colours.GRAY2
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 315),
                        new Dimension(GridSizeMode.Absolute, 242),
                        new Dimension(GridSizeMode.Absolute, 242),
                        new Dimension(GridSizeMode.Absolute, 135),
                        new Dimension(GridSizeMode.Absolute, 130),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new InstanceSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = remoteModuleSource.DisplayName
                            },
                            new LatestVersionSpriteText(remoteModuleSource)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            },
                            new InstanceSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = remoteModuleSource.GetInstalledVersion()
                            },
                            new InstanceSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = remoteModuleSource.SourceType.ToString()
                            },
                            new InstanceSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            }
                        }
                    }
                }
            }
        };
    }

    private partial class InstanceSpriteText : SpriteText
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Font = Fonts.REGULAR.With(size: 27);
            Colour = Colours.WHITE0;
        }
    }

    private partial class LatestVersionSpriteText : InstanceSpriteText
    {
        private readonly RemoteModuleSource remoteModuleSource;

        public LatestVersionSpriteText(RemoteModuleSource remoteModuleSource)
        {
            this.remoteModuleSource = remoteModuleSource;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (remoteModuleSource.IsIncompatible())
            {
                Text = "Incompatible";
                Colour = Colours.ORANGE1;
            }
            else if (remoteModuleSource.IsUnavailable())
            {
                Text = "Unavailable";
                Colour = Colours.RED1;
            }
            else
            {
                Text = remoteModuleSource.LatestRelease!.TagName;
            }
        }
    }
}
