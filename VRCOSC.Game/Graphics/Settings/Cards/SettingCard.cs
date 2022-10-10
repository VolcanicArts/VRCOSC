// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public abstract class SettingCard<T> : Container
{
    private VRCOSCButton resetToDefault = null!;
    protected FillFlowContainer ContentFlow = null!;

    private readonly string title;
    private readonly string description;
    protected readonly Bindable<T> SettingBindable;

    protected SettingCard(string title, string description, Bindable<T> settingBindable)
    {
        this.title = title;
        this.description = description;
        SettingBindable = settingBindable;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
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
                Size = new Vector2(30, 50),
                Padding = new MarginPadding(5),
                Child = resetToDefault = new BasicButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1.0f),
                    CornerRadius = 5,
                    CornerExponent = 2,
                    Action = setDefault,
                    BackgroundColour = VRCOSCColour.Blue,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = VRCOSCColour.BlueLight,
                        Radius = 5
                    }
                }
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                BorderColour = VRCOSCColour.Gray0,
                BorderThickness = 2,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = VRCOSCColour.Gray2
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
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
                            ContentFlow = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 10),
                            }
                        }
                    }
                }
            }
        };
        textFlow.AddText(title, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 25);
        });
        textFlow.AddParagraph(description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = VRCOSCColour.Gray9;
        });
    }

    protected override void LoadComplete()
    {
        SettingBindable.ValueChanged += e => Schedule(() => performAttributeUpdate(e));
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
            BorderColour = VRCOSCColour.Gray0,
            BorderThickness = 2
        };
    }

    #endregion
}
