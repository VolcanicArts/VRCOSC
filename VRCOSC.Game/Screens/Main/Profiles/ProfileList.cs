// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.List;

namespace VRCOSC.Game.Screens.Main.Profiles;

public partial class ProfileList : HeightLimitedScrollableList<ProfileListInstance>
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Resolved]
    private ProfilesPage profilesPage { get; set; } = null!;

    protected override Colour4 BackgroundColourOdd => Colours.GRAY2;
    protected override Colour4 BackgroundColourEven => Colours.GRAY4;

    protected override Drawable CreateHeader() => new Container
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 50,
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = Fonts.BOLD.With(size: 28),
                    Text = "Profiles"
                }
            }
        }
    };

    protected override Drawable CreateFooter() => new Container
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        RelativeSizeAxes = Axes.X,
        Height = 46,
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(140, 36),
                Icon = FontAwesome.Solid.Plus,
                IconSize = 20,
                BackgroundColour = Colours.GREEN0,
                CornerRadius = 5,
                Action = () => profilesPage.CreateProfile()
            }
        }
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        appManager.ProfileManager.Profiles.BindCollectionChanged(onProfileCollectionChanged, true);
    }

    private void onProfileCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Clear();
        appManager.ProfileManager.Profiles.OrderBy(profile => profile.Name.Value).ForEach(profile => Add(new ProfileListInstance(profile)));
    }
}
