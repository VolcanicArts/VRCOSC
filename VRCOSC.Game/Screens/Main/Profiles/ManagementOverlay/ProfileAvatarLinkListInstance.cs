// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;
using VRCOSC.Graphics.UI.Text;
using VRCOSC.Profiles;

namespace VRCOSC.Screens.Main.Profiles.ManagementOverlay;

public partial class ProfileAvatarLinkListInstance : Container
{
    private readonly Profile parentProfile;
    private readonly Bindable<string> avatarLinkBindable;
    private readonly bool even;

    public ProfileAvatarLinkListInstance(Profile parentProfile, Bindable<string> avatarLinkBindable, bool even)
    {
        this.parentProfile = parentProfile;
        this.avatarLinkBindable = avatarLinkBindable;
        this.even = even;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 45;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = even ? Colours.GRAY5 : Colours.GRAY2
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension(GridSizeMode.Absolute, 35)
                    },
                    Content = new[]
                    {
                        new Drawable?[]
                        {
                            new InstanceTextBox
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                Current = avatarLinkBindable.GetBoundCopy()
                            },
                            null,
                            new IconButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Icon = FontAwesome.Solid.Minus,
                                Size = new Vector2(0.9f),
                                CornerRadius = 5,
                                BackgroundColour = Colours.RED0,
                                Action = () => parentProfile.LinkedAvatars.Remove(avatarLinkBindable)
                            }
                        }
                    }
                }
            }
        };
    }

    private partial class InstanceTextBox : TextBox
    {
        public InstanceTextBox()
        {
            BackgroundUnfocused = Colours.GRAY7;
            BackgroundFocused = Colours.GRAY7;
            BackgroundCommit = Colours.GRAY7;
        }
    }
}
