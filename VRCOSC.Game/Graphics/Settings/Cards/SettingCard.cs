// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public abstract partial class SettingCard<T> : Container
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    protected override FillFlowContainer Content { get; }

    protected readonly Bindable<T> SettingBindable;

    private readonly IconButton resetToDefault;

    protected SettingCard(string title, string description, Bindable<T> settingBindable, string linkedUrl)
    {
        SettingBindable = settingBindable;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        TextFlowContainer textFlow;

        Container helpButton;

        InternalChildren = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Size = new Vector2(32),
                Padding = new MarginPadding(5),
                Depth = float.MinValue,
                Child = resetToDefault = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Action = settingBindable.SetDefault,
                    BorderThickness = 2,
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                    Icon = FontAwesome.Solid.Undo,
                    IconPadding = 6,
                    IconShadow = true,
                    Circular = true
                }
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                BorderThickness = 2,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ThemeManager.Current[ThemeAttribute.Light]
                    },
                    helpButton = new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Size = new Vector2(35),
                        FillMode = FillMode.Fit,
                        Padding = new MarginPadding(5),
                        Depth = float.MinValue,
                        Child = UIPrefabs.QuestionButton.With(d =>
                        {
                            d.IconPadding = 4;
                            d.Action = () => host.OpenUrlExternally(linkedUrl);
                        })
                    },
                    Content = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(5),
                        AutoSizeEasing = Easing.OutQuint,
                        AutoSizeDuration = 150,
                        Children = new Drawable[]
                        {
                            textFlow = new TextFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Bottom = 5,
                                    Horizontal = 5
                                },
                                AutoSizeEasing = Easing.OutQuint,
                                AutoSizeDuration = 150
                            }
                        }
                    }
                }
            }
        };

        textFlow.AddText(title, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        textFlow.AddParagraph(description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 15);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });

        helpButton.Alpha = string.IsNullOrEmpty(linkedUrl) ? 0 : 1;
    }

    protected override void Update()
    {
        resetToDefault.FadeTo(SettingBindable.IsDefault ? 0 : 1, 200, Easing.OutQuart);
    }

    protected override void LoadComplete()
    {
        SettingBindable.ValueChanged += performAttributeUpdate;
    }

    private void performAttributeUpdate(ValueChangedEvent<T> e)
    {
        UpdateValues(e.NewValue);
    }

    protected virtual void UpdateValues(T value)
    {
        SettingBindable.Value = value;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        SettingBindable.ValueChanged -= performAttributeUpdate;
    }
}
