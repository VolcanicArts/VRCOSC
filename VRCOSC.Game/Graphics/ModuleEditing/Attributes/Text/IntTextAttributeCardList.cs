// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;

public sealed partial class IntTextAttributeCardList : TextAttributeCardList
{
    public IntTextAttributeCardList(ModuleAttributeList attributeData)
        : base(attributeData)
    {
    }

    protected override Bindable<object> GetDefaultItem()
    {
        var value = AttributeData.AttributeList.LastOrDefault()?.Value ?? 0;
        return new Bindable<object>(value);
    }

    protected override object OnTextWrite(ValueChangedEvent<string> e)
    {
        if (string.IsNullOrEmpty(e.NewValue)) return 0;

        return int.TryParse(e.NewValue, out var intValue) ? intValue : e.OldValue;
    }
}
