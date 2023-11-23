// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Settings.Profiles;

public partial class ProfilesSection : Container
{
    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer<DrawableProfile> profileFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
                Padding = new MarginPadding
                {
                    Horizontal = 10,
                    Bottom = 10,
                    Top = 3,
                },
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new SpriteText
                        {
                            Font = Fonts.BOLD.With(size: 35),
                            Colour = Colours.WHITE2,
                            Text = "Profiles",
                            X = 3
                        }
                    },
                    new SettingsToggle(configManager.GetBindable<bool>(VRCOSCSetting.EnableAutomaticProfileSwitching), "Enable Automatic Switching", "Automatic switching changes your selected profile to one that is linked to the avatar you’re wearing when you change avatar. If none is found, the default profile is used"),
                    profileFlow = new FillFlowContainer<DrawableProfile>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5)
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(30),
                            Icon = FontAwesome.Solid.Plus,
                            BackgroundColour = Colours.GREEN0,
                            CornerRadius = 15,
                            Action = () => appManager.ProfileManager.Profiles.Add(new Profile())
                        }
                    }
                }
            }
        };

        appManager.ProfileManager.Profiles.BindCollectionChanged(onProfileCollectionChanged, true);
    }

    private void onProfileCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (Profile newProfile in e.NewItems)
            {
                profileFlow.Add(new DrawableProfile(newProfile));
            }
        }

        if (e.OldItems is not null)
        {
            foreach (Profile oldProfile in e.OldItems)
            {
                profileFlow.RemoveAll(drawableProfile => drawableProfile.ProfileID == oldProfile.ID, true);
            }
        }
    }
}
