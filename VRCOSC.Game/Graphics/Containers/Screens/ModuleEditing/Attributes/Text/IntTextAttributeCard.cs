// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Text;

public class IntTextAttributeCard : TextAttributeCard
{
    public IntTextAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    protected override object OnTextWrite(ValueChangedEvent<string> e)
    {
        if (string.IsNullOrEmpty(e.NewValue)) return 0;

        return int.TryParse(e.NewValue, out var intValue) ? intValue : e.OldValue;
    }
}
