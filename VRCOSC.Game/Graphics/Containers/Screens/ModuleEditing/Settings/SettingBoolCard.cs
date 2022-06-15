// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;

public class SettingBoolCard : SettingBaseCard
{
    public SettingBoolCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

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
                CornerRadius = 10,
                State = { Value = (bool)attributeData.Attribute.Value }
            }
        });

        checkBox.State.BindValueChanged(e => updateSetting(e.NewValue), true);

        ResetToDefault.Action += () =>
        {
            var defaultValue = (bool)attributeData.Attribute.Default;
            updateSetting(defaultValue);
            checkBox.State.Value = defaultValue;
        };
    }

    private void updateSetting(bool newValue)
    {
        attributeData.Attribute.Value = newValue;

        if (!attributeData.Attribute.IsDefault)
        {
            ResetToDefault.Show();
        }
        else
        {
            ResetToDefault.Hide();
        }
    }
}
