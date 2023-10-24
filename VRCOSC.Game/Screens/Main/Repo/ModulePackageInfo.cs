// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.Remote;

namespace VRCOSC.Game.Screens.Main.Repo;

public partial class ModulePackageInfo : VisibilityContainer
{
    private const int cover_height = 156;

    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;

    private Sprite coverSprite = null!;
    private SpriteText title = null!;
    private SpriteText author = null!;
    private TextFlowContainer description = null!;

    public Bindable<RemoteModuleSource?> CurrentRemoteModuleSource { get; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(650, 520),
                Masking = true,
                CornerRadius = 10,
                EdgeEffect = new EdgeEffectParameters
                {
                    Radius = 10,
                    Colour = Colours.Black,
                    Type = EdgeEffectType.Shadow,
                    Offset = Vector2.Zero
                },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colours.GRAY2
                    },
                    new GridContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = cover_height,
                                    Child = coverSprite = new Sprite
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.Both,
                                        FillMode = FillMode.Fill
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 3,
                                    Colour = Colours.GRAY0
                                }
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Size = new Vector2(40),
                                            Child = new IconButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Size = new Vector2(36),
                                                CornerRadius = 5,
                                                Icon = FontAwesome.Solid.Undo,
                                                IconSize = 24,
                                                IconColour = Colours.WHITE0,
                                                BackgroundColour = Colours.RED0,
                                                Action = Hide
                                            }
                                        },
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Children = new Drawable[]
                                            {
                                                title = new SpriteText
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    Colour = Colours.WHITE0,
                                                    Font = Fonts.BOLD.With(size: 40)
                                                },
                                                author = new SpriteText
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    Colour = Colours.WHITE2,
                                                    Font = Fonts.REGULAR.With(size: 17)
                                                },
                                                new Container
                                                {
                                                    Anchor = Anchor.TopCentre,
                                                    Origin = Anchor.TopCentre,
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Padding = new MarginPadding(10),
                                                    Child = description = new TextFlowContainer(t =>
                                                    {
                                                        t.Colour = Colours.WHITE0;
                                                        t.Font = Fonts.REGULAR.With(size: 30);
                                                    })
                                                    {
                                                        Anchor = Anchor.TopCentre,
                                                        Origin = Anchor.TopCentre,
                                                        TextAnchor = Anchor.TopCentre,
                                                        RelativeSizeAxes = Axes.Both
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },

                    new Container
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 80,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colours.GRAY0
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Padding = new MarginPadding(10),
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5, 0),
                                Children = new Drawable[]
                                {
                                    new FooterButton
                                    {
                                        Icon = FontAwesome.Brands.Github,
                                        BackgroundColour = Color4Extensions.FromHex("333"),
                                        Action = () => host.OpenUrlExternally(CurrentRemoteModuleSource.Value!.Repository!.HtmlUrl)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        CurrentRemoteModuleSource.BindValueChanged(_ => onCurrentRemoteModuleSourceChange());
    }

    private async void onCurrentRemoteModuleSourceChange()
    {
        if (CurrentRemoteModuleSource.Value is null) return;

        title.Text = CurrentRemoteModuleSource.Value.DisplayName;
        author.Text = $"Created by {CurrentRemoteModuleSource.Value.RepositoryOwner}";
        description.Text = CurrentRemoteModuleSource.Value.Repository?.Description ?? string.Empty;

        var coverUrl = CurrentRemoteModuleSource.Value.GetCoverUrl();
        if (coverUrl is null) return;

        var texture = await game.Textures.GetAsync(coverUrl, CancellationToken.None);
        var textureCrop = new RectangleF(new Vector2(0, texture.Height / 2f - cover_height), new Vector2(texture.Width, cover_height * 2));
        coverSprite.Texture = texture.Crop(textureCrop);
        coverSprite.FillAspectRatio = coverSprite.Texture.Width / (float)coverSprite.Texture.Height;
        coverSprite.FadeInFromZero(500, Easing.OutQuart);
    }

    protected override void PopIn()
    {
        this.ScaleTo(1.1f).ScaleTo(1f, 500, Easing.OutQuart);
        this.FadeInFromZero(500, Easing.OutQuart);
        coverSprite.Alpha = 0;
    }

    protected override void PopOut()
    {
        this.ScaleTo(0.9f, 500, Easing.OutQuart);
        this.FadeOutFromOne(500, Easing.OutQuart);
        Scheduler.AddDelayed(() => CurrentRemoteModuleSource.Value = null, 500);
    }

    private partial class FooterButton : Container
    {
        public required IconUsage Icon { get; init; }
        public required Color4 BackgroundColour { get; init; }

        public Action? Action;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            Size = new Vector2(60);

            Child = new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 10,
                BackgroundColour = BackgroundColour,
                Icon = Icon,
                IconSize = 42,
                IconColour = Colours.WHITE0,
                Action = Action
            };
        }
    }
}
