// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Profiles;

[Cached]
public partial class ProfileDropdown : ClickableContainer
{
    private ProfileDropdownContent dropdownContent = null!;

    private readonly BindableBool open = new();

    public Bindable<Profile> ProfileBindable { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new ProfileDropdownHeader(),
            dropdownContent = new ProfileDropdownContent()
        };

        Action += () => open.Toggle();
        open.BindValueChanged(onOpenStateChanged, true);
    }

    public override bool AcceptsFocus => true;
    protected override void OnFocusLost(FocusLostEvent e) => open.Value = false;

    private void onOpenStateChanged(ValueChangedEvent<bool> e)
    {
        if (e.NewValue)
        {
            dropdownContent.MoveToY(5, 100, Easing.OutQuint);
            dropdownContent.ScaleTo(Vector2.One, 100, Easing.OutQuint);
        }
        else
        {
            dropdownContent.MoveToY(0, 100, Easing.OutQuint);
            dropdownContent.ScaleTo(new Vector2(1, 0), 100, Easing.OutQuint);
        }
    }
}

public partial class ProfileDropdownHeader : Container
{
    [Resolved]
    private ProfileDropdown profileDropdown { get; set; } = null!;

    private Box background = null!;
    private SpriteText headerText = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        Height = 35;
        Masking = true;
        CornerRadius = 5;

        Children = new Drawable[]
        {
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY2
            },
            new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Padding = new MarginPadding(3),
                Spacing = new Vector2(2, 0),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Child = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.7f),
                            Icon = FontAwesome.Solid.ChevronDown,
                            Colour = Colours.WHITE2
                        }
                    },
                    headerText = new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = Fonts.REGULAR.With(size: 27),
                        Colour = Colours.WHITE2
                    }
                }
            }
        };

        profileDropdown.ProfileBindable.BindValueChanged(onDefaultProfileChange);
        profileDropdown.ProfileBindable.Value.Name.BindValueChanged(onDefaultProfileNameChange, true);
    }

    private void onDefaultProfileChange(ValueChangedEvent<Profile> e)
    {
        e.OldValue.Name.ValueChanged -= onDefaultProfileNameChange;
        e.NewValue.Name.BindValueChanged(onDefaultProfileNameChange, true);
    }

    private void onDefaultProfileNameChange(ValueChangedEvent<string> e)
    {
        headerText.Text = e.NewValue;
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(Colours.GRAY6, 100, Easing.OutQuint);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        background.FadeColour(Colours.GRAY2, 100, Easing.OutQuint);
    }
}

public partial class ProfileDropdownContent : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    protected override FillFlowContainer Content { get; }

    public ProfileDropdownContent()
    {
        Anchor = Anchor.BottomCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;
        BorderThickness = 3;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY2
            },
            Content = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(7),
                Spacing = new Vector2(0, 2)
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        appManager.ProfileManager.Profiles.BindCollectionChanged(onProfileCollectionChange, true);
    }

    private void onProfileCollectionChange(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Clear();
        AddRange(appManager.ProfileManager.Profiles.Select(profile => new ProfileDropdownItem(profile)));
    }

    protected override bool OnClick(ClickEvent e) => true;
}

public partial class ProfileDropdownItem : Container
{
    [Resolved]
    private ProfileDropdown profileDropdown { get; set; } = null!;

    private readonly Profile profile;

    private Box background = null!;
    private SpriteText nameText = null!;

    public ProfileDropdownItem(Profile profile)
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

        Children = new Drawable[]
        {
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY2
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(2),
                Child = nameText = new SpriteText
                {
                    Font = Fonts.REGULAR.With(size: 23),
                    X = 2
                }
            }
        };

        profile.Name.BindValueChanged(onProfileNameChanged, true);
    }

    private void onProfileNameChanged(ValueChangedEvent<string> e)
    {
        nameText.Text = e.NewValue;
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeColour(Colours.GRAY5, 100, Easing.OutQuart);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        background.FadeColour(Colours.GRAY2, 100, Easing.OutQuart);
    }

    protected override bool OnClick(ClickEvent e)
    {
        GetContainingInputManager().ChangeFocus(null);
        profileDropdown.ProfileBindable.Value = profile;
        return true;
    }
}
