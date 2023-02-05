// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterInstance : Container
{
    [Resolved]
    private RouterManager routerManager { get; set; } = null!;

    private readonly RouterData data;

    public RouterInstance(RouterData data)
    {
        this.data = data;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreLeft,
                Size = new Vector2(30, 60),
                Padding = new MarginPadding(5),
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Action = () =>
                    {
                        routerManager.Store.Remove(data);
                        this.RemoveAndDisposeImmediately();
                    },
                    IconPadding = 4,
                    CornerRadius = 10,
                    BorderThickness = 2,
                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Failure],
                    Icon = FontAwesome.Solid.Get(0xf00d),
                    IconShadow = true
                }
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                BorderThickness = 2,
                BorderColour = Colour4.Black,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = ThemeManager.Current[ThemeAttribute.Darker],
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(4),
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize)
                                },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension()
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new StringTextBox
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.X,
                                            Height = 35,
                                            Masking = true,
                                            CornerRadius = 10,
                                            BorderThickness = 2,
                                            BorderColour = Colour4.Black,
                                            Text = data.Label,
                                            OnValidEntry = entryData => data.Label = entryData,
                                            PlaceholderText = "Enter a label",
                                            MinimumLength = 1
                                        }
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = "Forward data from ",
                                        Font = FrameworkFont.Regular.With(size: 30),
                                        Colour = ThemeManager.Current[ThemeAttribute.Text]
                                    },
                                    new IPPortTextBox
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(200, 35),
                                        Masking = true,
                                        CornerRadius = 10,
                                        BorderThickness = 2,
                                        BorderColour = Colour4.Black,
                                        Text = string.IsNullOrEmpty(data.Endpoints.ReceiveAddress) ? string.Empty : $"{data.Endpoints.ReceiveAddress}:{data.Endpoints.ReceivePort}",
                                        OnValidEntry = entryData =>
                                        {
                                            data.Endpoints.ReceiveAddress = entryData.IP;
                                            data.Endpoints.ReceivePort = entryData.Port;
                                        }
                                    },
                                    new SpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = " to VRChat",
                                        Font = FrameworkFont.Regular.With(size: 30),
                                        Colour = ThemeManager.Current[ThemeAttribute.Text]
                                    },
                                }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = "Forward data from VRChat to ",
                                        Font = FrameworkFont.Regular.With(size: 30),
                                        Colour = ThemeManager.Current[ThemeAttribute.Text]
                                    },
                                    new IPPortTextBox
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(200, 35),
                                        Masking = true,
                                        CornerRadius = 10,
                                        BorderThickness = 2,
                                        BorderColour = Colour4.Black,
                                        Text = string.IsNullOrEmpty(data.Endpoints.SendAddress) ? string.Empty : $"{data.Endpoints.SendAddress}:{data.Endpoints.SendPort}",
                                        OnValidEntry = entryData =>
                                        {
                                            data.Endpoints.SendAddress = entryData.IP;
                                            data.Endpoints.SendPort = entryData.Port;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
