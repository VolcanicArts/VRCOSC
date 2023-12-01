// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.List;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Profiles;

public partial class ProfileListInstance : HeightLimitedScrollableListItem
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Resolved]
    private ProfilesPage profilesPage { get; set; } = null!;

    private readonly Profile profile;

    private readonly SpriteText nameText;
    private readonly IconButton removeButton;

    public ProfileListInstance(Profile profile)
    {
        this.profile = profile;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Children = new Drawable[]
        {
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    nameText = new SpriteText
                    {
                        Font = Fonts.REGULAR.With(size: 25)
                    }
                }
            },
            new FillFlowContainer
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8, 0),
                Padding = new MarginPadding(5),
                Children = new Drawable[]
                {
                    removeButton = new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Icon = FontAwesome.Solid.Minus,
                        BackgroundColour = Colours.RED0,
                        CornerRadius = 5,
                        Action = () => appManager.ProfileManager.Profiles.Remove(profile)
                    },
                    new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Icon = FontAwesome.Solid.Edit,
                        BackgroundColour = Colours.BLUE0,
                        CornerRadius = 5,
                        Action = () => profilesPage.EditProfile(profile)
                    }
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        profile.Name.BindValueChanged(e => nameText.Text = e.NewValue, true);
        removeButton.Alpha = appManager.ProfileManager.Profiles.Count > 1 ? 1 : 0;
    }
}
