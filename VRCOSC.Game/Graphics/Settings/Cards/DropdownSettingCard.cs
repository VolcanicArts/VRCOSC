// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public class DropdownSettingCard<T> : SettingCard<T>
{
    public DropdownSettingCard(string title, string description, Bindable<T> settingBindable)
        : base(title, description, settingBindable)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ContentFlow.Add(new VRCOSCDropdown<T>()
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues(typeof(T)).Cast<T>(),
            Current = SettingBindable
        });
    }

    // SettingBindable is being bound to by VRCOSCDropdown so this isn't needed
    protected override void UpdateValues(T value) { }
}
