// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;

public class SettingBoolCard : SettingBaseCard
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Checkbox checkBox;

        Add(new Container
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            Padding = new MarginPadding(15),
            Child = checkBox = new Checkbox
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                State = { Value = SourceModule.DataManager.GetSettingAs<bool>(Key) }
            }
        });

        checkBox.State.BindValueChanged((e) => updateSetting(e.NewValue), true);

        ResetToDefault.Action += () =>
        {
            var defaultValue = SourceModule.GetDefaultSetting<bool>(Key);
            updateSetting(defaultValue);
            checkBox.State.Value = defaultValue;
        };
    }

    private void updateSetting(bool newValue)
    {
        SourceModule.DataManager.UpdateBoolSetting(Key, newValue);

        if (!SourceModule.GetDefaultSetting<bool>(Key).Equals(SourceModule.DataManager.GetSettingAs<bool>(Key)))
        {
            ResetToDefault.Show();
        }
        else
        {
            ResetToDefault.Hide();
        }
    }
}
