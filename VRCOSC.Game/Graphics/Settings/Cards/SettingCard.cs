// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public abstract partial class SettingCard<T> : Container
{
    private readonly VRCOSCButton resetToDefault;

    protected readonly Container ContentWrapper;
    protected override FillFlowContainer Content { get; }

    protected readonly Bindable<T> SettingBindable;

    protected SettingCard(string title, string description, Bindable<T> settingBindable)
    {
        SettingBindable = settingBindable;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        TextFlowContainer textFlow;

        InternalChildren = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreRight,
                Size = new Vector2(30, 60),
                Padding = new MarginPadding(5),
                Child = resetToDefault = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Action = setDefault,
                    IconPadding = 4,
                    CornerRadius = 10,
                    BorderThickness = 2,
                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                    Icon = FontAwesome.Solid.Undo,
                    IconShadow = true
                }
            },
            ContentWrapper = new Container
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
                        Colour = ThemeManager.Current[ThemeAttribute.Darker]
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            textFlow = new TextFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.TopLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            },
                            Content = new FillFlowContainer
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
                }
            }
        };

        textFlow.AddText(title, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 25);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });
        textFlow.AddParagraph(description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });
    }

    protected override void LoadComplete()
    {
        SettingBindable.ValueChanged += e => Schedule(performAttributeUpdate, e);
        resetToDefault.FadeTo(!SettingBindable.IsDefault ? 1 : 0);
    }

    private void performAttributeUpdate(ValueChangedEvent<T> e)
    {
        UpdateValues(e.NewValue);
        updateResetToDefault(!SettingBindable.IsDefault);
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

    private void setDefault()
    {
        SettingBindable.SetDefault();
    }

    private void updateResetToDefault(bool show)
    {
        resetToDefault.FadeTo(show ? 1 : 0, 200, Easing.OutQuart);
    }

    #region Graphics

    protected static VRCOSCTextBox CreateTextBox()
    {
        return new VRCOSCTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Masking = true,
            CornerRadius = 5,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2
        };
    }

    #endregion
}
