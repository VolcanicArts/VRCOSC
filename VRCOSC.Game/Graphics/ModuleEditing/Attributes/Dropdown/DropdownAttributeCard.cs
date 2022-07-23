// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Dropdown;

public class DropdownAttributeCard<T> : AttributeCard where T : Enum
{
    protected VRCOSCDropdown<T> Dropdown = null!;

    public DropdownAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddToFlow(Dropdown = new VRCOSCDropdown<T>()
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues(typeof(T)).Cast<T>()
        });

        AttributeData.Attribute.ValueChanged += e => updateValues(e.NewValue);
        Dropdown.Current.ValueChanged += e => updateValues(OnDropdownSelect(e));
    }

    private void updateValues(object value)
    {
        AttributeData.Attribute.Value = value;
        Dropdown.Current.Value = (T)value;
    }

    protected virtual object OnDropdownSelect(ValueChangedEvent<T> e)
    {
        return e.NewValue;
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateValues;
        base.Dispose(isDisposing);
    }
}
