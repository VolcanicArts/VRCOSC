// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Profiles.ManagementOverlay;

public partial class ProfileManagementOverlay : VisibilityContainer
{
    private readonly Regex avatarIDRegex = new("avtr_[a-zA-Z0-9]{8}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{12}");

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;

    public Bindable<bool> Editing = new();

    private SpriteText headerText = null!;
    private TextButton saveButton = null!;
    private FillFlowContainer contentFlow = null!;

    private Profile? originalProfile;
    private Profile editingProfile = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(0.65f),
            Masking = true,
            CornerRadius = 5,
            BorderThickness = 3,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY4
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(3),
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 60),
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 60)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Colours.GRAY0
                                        },
                                        headerText = new SpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Font = Fonts.BOLD.With(size: 35),
                                            Colour = Colours.WHITE2
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new BasicScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ClampExtension = 0,
                                    ScrollbarVisible = false,
                                    Padding = new MarginPadding(10),
                                    ScrollContent =
                                    {
                                        Child = contentFlow = new FillFlowContainer
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 10)
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Colours.GRAY0
                                        },
                                        saveButton = new TextButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Size = new Vector2(240, 34),
                                            TextContent = "Save",
                                            TextFont = Fonts.REGULAR.With(size: 30),
                                            BackgroundColour = Colours.BLUE0,
                                            CornerRadius = 17,
                                            Enabled = { Value = false },
                                            Action = OnSave
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        Editing.BindValueChanged(e => headerText.Text = e.NewValue ? "Edit Profile" : "Create Profile", true);
    }

    protected override void Update()
    {
        var canSave = !string.IsNullOrEmpty(editingProfile.Name.Value) &&
                      appManager.ProfileManager.Profiles.All(profile => profile.Name.Value != editingProfile.Name.Value) &&
                      editingProfile.LinkedAvatars.All(linkedAvatar => avatarIDRegex.IsMatch(linkedAvatar.Value));

        saveButton.Enabled.Value = canSave;
    }

    public void SetProfile(Profile? profile)
    {
        originalProfile = profile;
        editingProfile = profile is null ? new Profile() : profile.Clone();

        contentFlow.Clear();
        contentFlow.AddRange(new Drawable[]
        {
            new ProfileNameContainer(editingProfile),
            new ProfileAvatarLinkList(editingProfile),
            new IconButton
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Size = new Vector2(100, 34),
                Icon = FontAwesome.Solid.Plus,
                BackgroundColour = Colours.GREEN0,
                CornerRadius = 17,
                Action = () => editingProfile.LinkedAvatars.Add(new Bindable<string>(string.Empty))
            }
        });
    }

    private void OnSave()
    {
        if (originalProfile is not null)
        {
            var foundProfile = appManager.ProfileManager.Profiles.First(profile => profile.ID.Equals(editingProfile.ID));
            appManager.ProfileManager.Profiles.Remove(foundProfile);
        }

        appManager.ProfileManager.Profiles.Add(editingProfile.Clone());

        Hide();
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(250, Easing.OutCubic);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(250, Easing.OutCubic);
    }
}
