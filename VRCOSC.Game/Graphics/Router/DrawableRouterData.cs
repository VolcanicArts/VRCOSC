// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Router;

public partial class DrawableRouterData : RouterDataFlowEntry
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private readonly RouterData data;

    public DrawableRouterData(RouterData data)
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
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                BorderThickness = 2,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = ThemeManager.Current[ThemeAttribute.Darker],
                        RelativeSizeAxes = Axes.Both
                    },
                    new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Size = new Vector2(35),
                        Padding = new MarginPadding(5),
                        Depth = float.MinValue,
                        Child = new IconButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Icon = FontAwesome.Solid.Get(0xf00d),
                            IconPadding = 6,
                            Circular = true,
                            IconShadow = true,
                            Action = () =>
                            {
                                appManager.RouterManager.Store.Remove(data);
                                this.RemoveAndDisposeImmediately();
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(5),
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Width = 0.75f,
                                Child = new StringTextBox
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 25,
                                    Masking = true,
                                    CornerRadius = 5,
                                    BorderThickness = 2,
                                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                    Text = data.Label.Value,
                                    OnValidEntry = entryData => data.Label.Value = entryData,
                                    PlaceholderText = "Enter a label",
                                    MinimumLength = 1
                                }
                            },
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Masking = true,
                                CornerRadius = 10,
                                BorderThickness = 2,
                                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = ThemeManager.Current[ThemeAttribute.Mid],
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 5),
                                        Padding = new MarginPadding
                                        {
                                            Vertical = 5,
                                            Horizontal = 1
                                        },
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Children = new Drawable[]
                                                {
                                                    new IPPortTextBox
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Size = new Vector2(150, 25),
                                                        Masking = true,
                                                        CornerRadius = 5,
                                                        BorderThickness = 2,
                                                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                                        Text = string.IsNullOrEmpty(data.Endpoints.ReceiveAddress.Value) ? string.Empty : $"{data.Endpoints.ReceiveAddress.Value}:{data.Endpoints.ReceivePort.Value}",
                                                        OnValidEntry = entryData =>
                                                        {
                                                            data.Endpoints.ReceiveAddress.Value = entryData.Address.ToString();
                                                            data.Endpoints.ReceivePort.Value = entryData.Port;
                                                        }
                                                    },
                                                    new SpriteText
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Text = " to VRChat",
                                                        Font = FrameworkFont.Regular.With(size: 20),
                                                        Colour = ThemeManager.Current[ThemeAttribute.Text]
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
                                                        Text = "VRChat to ",
                                                        Font = FrameworkFont.Regular.With(size: 20),
                                                        Colour = ThemeManager.Current[ThemeAttribute.Text]
                                                    },
                                                    new IPPortTextBox
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Size = new Vector2(150, 25),
                                                        Masking = true,
                                                        CornerRadius = 5,
                                                        BorderThickness = 2,
                                                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                                                        Text = string.IsNullOrEmpty(data.Endpoints.SendAddress.Value) ? string.Empty : $"{data.Endpoints.SendAddress.Value}:{data.Endpoints.SendPort.Value}",
                                                        OnValidEntry = entryData =>
                                                        {
                                                            data.Endpoints.SendAddress.Value = entryData.Address.ToString();
                                                            data.Endpoints.SendPort.Value = entryData.Port;
                                                        }
                                                    }
                                                }
                                            }
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
