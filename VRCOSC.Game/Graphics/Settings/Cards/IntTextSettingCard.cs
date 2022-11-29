// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public sealed partial class IntTextSettingCard : SettingCard<int>
{
    private VRCOSCTextBox textBox = null!;

    public IntTextSettingCard(string title, string description, Bindable<int> settingBindable)
        : base(title, description, settingBindable)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(textBox = CreateTextBox().With(t => t.Text = SettingBindable.Value.ToString()));
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        textBox.Current.ValueChanged += e => UpdateValues(OnTextWrite(e));
    }

    protected override void UpdateValues(int value)
    {
        base.UpdateValues(value);
        textBox.Current.Value = value.ToString();
    }

    private static int OnTextWrite(ValueChangedEvent<string> e)
    {
        if (string.IsNullOrEmpty(e.NewValue)) return 0;

        return int.TryParse(e.NewValue, out var intValue) ? intValue : int.Parse(e.OldValue);
    }
}
