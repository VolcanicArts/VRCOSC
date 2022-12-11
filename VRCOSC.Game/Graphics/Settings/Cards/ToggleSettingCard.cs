// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public partial class ToggleSettingCard : SettingCard<bool>
{
    private ToggleButton toggleButton = null!;

    public ToggleSettingCard(string title, string description, Bindable<bool> settingBindable)
        : base(title, description, settingBindable)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ContentWrapper.Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Padding = new MarginPadding(10),
            Child = toggleButton = new ToggleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 10,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                BorderThickness = 2,
                ShouldAnimate = false,
                State = { Value = SettingBindable.Value }
            }
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        toggleButton.State.ValueChanged += e => UpdateValues(e.NewValue);
    }

    protected override void UpdateValues(bool value)
    {
        base.UpdateValues(value);
        toggleButton.State.Value = value;
    }
}
