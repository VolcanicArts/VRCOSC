// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.Settings.Cards;

public partial class TextSettingCard<TTextBox, TType> : SettingCard<TType> where TTextBox : ValidationTextBox<TType>, new()
{
    private TTextBox textBox = null!;

    public TextSettingCard(string title, string description, Bindable<TType> settingBindable, string linkedUrl)
        : base(title, description, settingBindable, linkedUrl)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(textBox = new TTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 25,
            Masking = true,
            CornerRadius = 5,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2,
            Text = SettingBindable.Value?.ToString(),
            EmptyIsValid = false
        });

        SettingBindable.ValueChanged += _ => textBox.Text = SettingBindable.Value?.ToString();
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        textBox.OnValidEntry += UpdateValues;
    }
}
