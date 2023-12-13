// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Graphics;
using VRCOSC.Profiles;

namespace VRCOSC.Screens.Main.Profiles;

public partial class ActiveDropdownContainer : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private Bindable<Profile> proxyActiveProfile = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        proxyActiveProfile = appManager.ProfileManager.ActiveProfile.GetUnboundCopy();

        proxyActiveProfile.BindValueChanged(e => appManager.ChangeProfile(e.NewValue));
        appManager.ProfileManager.ActiveProfile.BindValueChanged(e => proxyActiveProfile.Value = e.NewValue);

        Children = new Drawable[]
        {
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY0
                }
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 60,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = "Current Profile",
                        Font = Fonts.BOLD.With(size: 30),
                        Colour = Colours.WHITE0
                    },
                    new ProfileDropdown
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Y,
                        Width = 285,
                        ProfileBindable = proxyActiveProfile.GetBoundCopy()
                    }
                }
            }
        };
    }
}
