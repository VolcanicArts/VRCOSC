// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public partial class SliderSettingCard<TType> : SettingCard<TType> where TType : struct, IComparable<TType>, IConvertible, IEquatable<TType>
{
    private VRCOSCSlider<TType> slider = null!;

    public SliderSettingCard(string title, string description, BindableNumber<TType> settingBindable, string linkedUrl)
        : base(title, description, settingBindable, linkedUrl)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(slider = new VRCOSCSlider<TType>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 25,
            RoudedCurrent = (BindableNumber<TType>)SettingBindable.GetBoundCopy()
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        slider.RoudedCurrent.ValueChanged += e => UpdateValues(e.NewValue);
    }

    protected override void UpdateValues(TType value)
    {
        base.UpdateValues(value);
        slider.Current.Value = value;
    }
}
