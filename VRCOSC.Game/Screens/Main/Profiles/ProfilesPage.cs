// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Screens.Main.Profiles.ManagementOverlay;

namespace VRCOSC.Game.Screens.Main.Profiles;

[Cached]
public partial class ProfilesPage : Container
{
    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    private BufferedContainer bufferedContainer = null!;
    private Box backgroundDarkener = null!;
    private ProfileManagementOverlay profileManagementOverlay = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            bufferedContainer = new BufferedContainer
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColour = Colours.BLACK,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colours.GRAY1
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10),
                        Child = new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 8),
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 8),
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 8),
                                new Dimension()
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new ActiveDropdownContainer
                                    {
                                        Depth = -2
                                    }
                                },
                                null,
                                new Drawable[]
                                {
                                    new ProfilesToggle(configManager.GetBindable<bool>(VRCOSCSetting.AutomaticProfileSwitching), "Automatic Switching", "Automatic switching changes your selected profile to one that is linked to the avatar you’re wearing when you change avatar. If none is found, the default profile is used"),
                                },
                                null,
                                new Drawable[]
                                {
                                    new DefaultDropdownContainer
                                    {
                                        Depth = -1
                                    }
                                },
                                null,
                                new Drawable[]
                                {
                                    new ProfilesList()
                                }
                            }
                        }
                    }
                }
            },
            backgroundDarkener = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.Transparent
            },
            profileManagementOverlay = new ProfileManagementOverlay()
        };

        setupBlur();
    }

    private void setupBlur()
    {
        profileManagementOverlay.State.BindValueChanged(e =>
        {
            bufferedContainer.TransformTo(nameof(BufferedContainer.BlurSigma), e.NewValue == Visibility.Visible ? new Vector2(5) : new Vector2(0), 250, Easing.OutCubic);
            backgroundDarkener.FadeColour(e.NewValue == Visibility.Visible ? Colours.BLACK.Opacity(0.25f) : Colours.Transparent, 250, Easing.OutCubic);
        });
    }

    public void CreateProfile()
    {
        profileManagementOverlay.SetProfile(null);
        profileManagementOverlay.Show();
    }
}
