// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Settings.Profiles;

public partial class DrawableProfile : Container
{
    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    public Guid ProfileID => profile.ID;
    private readonly Profile profile;

    private Bindable<bool> avatarLinkBindable = null!;

    public DrawableProfile(Profile profile)
    {
        this.profile = profile;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;

        AvatarLinkContainer avatarLinkContainer;
        SpriteText defaultSpriteText;
        CheckBox defaultCheckBox;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY4
            },
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
                Padding = new MarginPadding(7),
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Width = 0.5f,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 75),
                            new Dimension()
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, 5),
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = Fonts.REGULAR.With(size: 25),
                                    Text = "Name:",
                                    X = 3
                                },
                                new StringTextBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 30,
                                    ValidCurrent = profile.Name.GetBoundCopy(),
                                    EmptyIsValid = true
                                }
                            },
                            null,
                            new Drawable[]
                            {
                                defaultSpriteText = new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = Fonts.REGULAR.With(size: 25),
                                    Text = "Default:",
                                    X = 3
                                },
                                defaultCheckBox = new CheckBox
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(30),
                                    IconSize = 20
                                }
                            }
                        }
                    },
                    avatarLinkContainer = new AvatarLinkContainer(profile)
                }
            }
        };

        avatarLinkBindable = configManager.GetBindable<bool>(VRCOSCSetting.EnableAutomaticProfileSwitching);
        avatarLinkBindable.BindValueChanged(e => avatarLinkContainer.Alpha = e.NewValue ? 1 : 0, true);

        defaultCheckBox.State.BindValueChanged(e =>
        {
            if (e.NewValue) appManager.ProfileManager.DefaultProfile.Value = profile;
        });

        appManager.ProfileManager.DefaultProfile.BindValueChanged(e => defaultCheckBox.State.Value = e.NewValue == profile, true);
        appManager.ProfileManager.Profiles.BindCollectionChanged((_, _) => defaultCheckBox.Alpha = defaultSpriteText.Alpha = appManager.ProfileManager.Profiles.Count > 1 ? 1 : 0, true);
    }
}
