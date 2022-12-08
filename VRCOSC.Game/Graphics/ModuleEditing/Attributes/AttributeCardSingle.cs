// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes;

public abstract partial class AttributeCardSingle : AttributeCard
{
    protected readonly ModuleAttributeSingle AttributeData;
    protected virtual bool ShouldLimitSaves => false;

    private DateTimeOffset lastUpdateTime;
    private readonly TimeSpan timeUntilSave = TimeSpan.FromSeconds(0.5f);

    private object lastValue = null!;

    protected AttributeCardSingle(ModuleAttributeSingle attributeData)
        : base(attributeData)
    {
        AttributeData = attributeData;
        lastUpdateTime = DateTimeOffset.Now;
    }

    protected override void LoadComplete()
    {
        AttributeData.Attribute.BindValueChanged(e => Schedule(performAttributeUpdate, e), true);
        lastValue = AttributeData.Attribute.Value;
    }

    protected override void Update()
    {
        if (lastUpdateTime + timeUntilSave < DateTimeOffset.Now && !AttributeData.Attribute.Value.Equals(lastValue))
        {
            AttributeData.Attribute.Value = lastValue;
        }
    }

    private void performAttributeUpdate(ValueChangedEvent<object> e)
    {
        UpdateValues(e.NewValue);
        UpdateResetToDefault(!AttributeData.IsDefault());
    }

    protected virtual void UpdateValues(object value)
    {
        lastValue = value;

        if (ShouldLimitSaves)
            lastUpdateTime = DateTimeOffset.Now;
        else
            AttributeData.Attribute.Value = value;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        AttributeData.Attribute.ValueChanged -= performAttributeUpdate;
    }
}
