// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;

public sealed partial class IntTextAttributeCard : TextAttributeCard
{
    public IntTextAttributeCard(ModuleAttributeSingle attributeData)
        : base(attributeData)
    {
    }

    protected override void OnTextBoxUpdate(ValueChangedEvent<string> e)
    {
        if (string.IsNullOrEmpty(e.NewValue))
        {
            UpdateAttribute(0);
            TextBox.Current.Value = "0";
        }

        if (int.TryParse(e.NewValue, out var intValue))
            UpdateAttribute(intValue);
        else
            TextBox.Current.Value = e.OldValue;
    }
}
