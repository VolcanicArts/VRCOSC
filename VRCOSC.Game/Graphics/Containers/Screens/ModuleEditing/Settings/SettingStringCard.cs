// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.TextBox;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;

public class SettingStringCard : SettingBaseCard
{
    [BackgroundDependencyLoader]
    private void load()
    {
        VRCOSCTextBox textBox;

        Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(0.5f, 1.0f),
            Padding = new MarginPadding(15),
            Child = textBox = new VRCOSCTextBox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                BorderThickness = 3,
                Text = SourceModule.DataManager.GetSettingAs<string>(Key)
            }
        });

        textBox.OnCommit = updateSetting;

        ResetToDefault.Action += () =>
        {
            var defaultValue = SourceModule.GetDefaultSetting<string>(Key);
            updateSetting(defaultValue);
            textBox.Text = defaultValue;
        };

        updateSetting(textBox.Text);
    }

    private void updateSetting(string newValue)
    {
        SourceModule.DataManager.UpdateStringSetting(Key, newValue);

        if (!SourceModule.GetDefaultSetting<string>(Key).Equals(SourceModule.DataManager.GetSettingAs<string>(Key)))
        {
            ResetToDefault.Show();
        }
        else
        {
            ResetToDefault.Hide();
        }
    }
}
