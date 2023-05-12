// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public abstract partial class SettingCard<T> : Container
{
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
    }

    protected override void LoadComplete()
    {
        SettingBindable.ValueChanged += e => Schedule(performAttributeUpdate, e);
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
