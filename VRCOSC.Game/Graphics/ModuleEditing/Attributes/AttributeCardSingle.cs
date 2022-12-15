// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes;

public abstract partial class AttributeCardSingle : AttributeCard
{
    protected new readonly ModuleAttributeSingle AttributeData;

    protected AttributeCardSingle(ModuleAttributeSingle attributeData)
        : base(attributeData)
    {
        AttributeData = attributeData;
    }

    protected override void LoadComplete()
    {
        AttributeData.Attribute.BindValueChanged(onAttributeUpdate, true);
    }

    private void onAttributeUpdate(ValueChangedEvent<object> e)
    {
        UpdateResetToDefault(!AttributeData.IsDefault());
    }

    protected virtual void UpdateAttribute(object value)
    {
        //Specifically check for equal values here to stop memory allocations from setting the bindable's value
        if (value == AttributeData.Attribute.Value) return;

        AttributeData.Attribute.Value = value;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        AttributeData.Attribute.ValueChanged -= onAttributeUpdate;
    }
}
