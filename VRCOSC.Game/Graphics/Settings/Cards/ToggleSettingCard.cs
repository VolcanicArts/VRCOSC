﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public partial class ToggleSettingCard : SettingCard<bool>
{
    private ToggleButton toggleButton = null!;

    public ToggleSettingCard(string title, string description, Bindable<bool> settingBindable, string linkedUrl)
        : base(title, description, settingBindable, linkedUrl)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(toggleButton = new ToggleButton
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Size = new Vector2(25),
            BorderThickness = 2,
            ShouldAnimate = false,
            State = { Value = SettingBindable.Value }
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
