// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Repo;

[Cached]
public partial class RepoTab : Container
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    public ModulePackageInfo PackageInfo { get; set; } = null!;

    private BufferedContainer bufferedContainer = null!;
    private ModulePackageList packageList = null!;
    private RepoTabHeader header = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            bufferedContainer = new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColour = Colours.BLACK,
                BlurSigma = Vector2.Zero,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colours.GRAY1
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10),
                        Child = new GridContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Absolute, 38),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension()
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    header = new RepoTabHeader
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    }
                                },
                                null,
                                new Drawable[]
                                {
                                    packageList = new ModulePackageList
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both
                                    }
                                }
                            }
                        }
                    }
                }
            },
            PackageInfo = new ModulePackageInfo
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both
            }
        };

        PackageInfo.State.BindValueChanged(e =>
        {
            bufferedContainer.TransformTo(nameof(BufferedContainer.BlurSigma), e.NewValue == Visibility.Visible ? new Vector2(2) : Vector2.Zero, 500, Easing.OutQuart);
            bufferedContainer.FadeColour(e.NewValue == Visibility.Visible ? Colour4.White.Darken(0.5f) : Colour4.White, 500, Easing.OutQuart);
        });

        game.OnListingRefresh += () =>
        {
            packageList.Refresh();
            header.Refresh();
        };
    }
}
