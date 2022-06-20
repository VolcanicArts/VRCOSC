// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class IntAttributeCard : StringAttributeCard
{
    public IntAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    protected override void OnTextChange(ValueChangedEvent<string> e)
    {
        if (string.IsNullOrEmpty(e.NewValue))
        {
            AttributeData.Attribute.Value = 0;
            textBox.Current.Value = "0";
            return;
        }

        if (int.TryParse(e.NewValue, out var intValue))
        {
            AttributeData.Attribute.Value = 0;
            textBox.Current.Value = intValue.ToString();
        }
        else
        {
            textBox.Current.Value = e.OldValue;
        }
    }
}
