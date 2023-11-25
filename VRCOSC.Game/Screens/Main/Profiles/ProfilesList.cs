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
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Profiles;

public partial class ProfilesList : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Resolved]
    private ProfilesPage profilesPage { get; set; } = null!;

    private readonly FillFlowContainer flowWrapper;
    private readonly BasicScrollContainer scrollContainer;
    private readonly Container header;

    protected override FillFlowContainer Content { get; }

    public ProfilesList()
    {
        RelativeSizeAxes = Axes.Both;

        InternalChild = flowWrapper = new FillFlowContainer
        {
            Name = "Flow Wrapper",
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Masking = true,
            CornerRadius = 5,
            Children = new Drawable[]
            {
                header = new Container
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
                },
                scrollContainer = new BasicScrollContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ClampExtension = 0,
                    ScrollbarVisible = false,
                    ScrollContent =
                    {
                        Child = Content = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical
                        }
                    }
                },
                new Container
                {
                    Name = "Footer",
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
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        appManager.ProfileManager.Profiles.BindCollectionChanged(onProfileCollectionChanged, true);
    }

    private void onProfileCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Clear();

        var even = false;

        appManager.ProfileManager.Profiles.OrderBy(profile => profile.Name.Value).ForEach(profile =>
        {
            Add(new ProfileListInstance(profile, even));
            even = !even;
        });
    }

    protected override void UpdateAfterChildren()
    {
        if (flowWrapper.DrawHeight >= DrawHeight)
        {
            scrollContainer.AutoSizeAxes = Axes.None;
            scrollContainer.Height = DrawHeight - header.DrawHeight - 5;
        }
        else
        {
            scrollContainer.AutoSizeAxes = Axes.Y;
        }
    }
}

public partial class ProfileListInstance : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Resolved]
    private ProfilesPage profilesPage { get; set; } = null!;

    private readonly Profile profile;

    private readonly SpriteText nameText;
    private readonly IconButton removeButton;

    public ProfileListInstance(Profile profile, bool even)
    {
        this.profile = profile;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = even ? Colours.GRAY4 : Colours.GRAY2
            },
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
