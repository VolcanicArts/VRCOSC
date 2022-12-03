// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public partial class TextSettingCard : SettingCard<string>
{
    private VRCOSCTextBox textBox = null!;

    public TextSettingCard(string title, string description, Bindable<string> settingBindable)
        : base(title, description, settingBindable)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(textBox = CreateTextBox().With(t => t.Text = SettingBindable.Value));
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        textBox.Current.ValueChanged += e => UpdateValues(e.NewValue);
    }

    protected override void UpdateValues(string value)
    {
        base.UpdateValues(value);
        textBox.Current.Value = value;
    }
}
