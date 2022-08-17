// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes.Dropdown;

public class DropdownAttributeCard<T> : AttributeCardSingle where T : Enum
{
    private VRCOSCDropdown<T> dropdown = null!;

    public DropdownAttributeCard(ModuleAttributeSingle attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        ContentFlow.Add(dropdown = new VRCOSCDropdown<T>()
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues(typeof(T)).Cast<T>()
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        dropdown.Current.ValueChanged += e => UpdateValues(e.NewValue);
    }

    protected override void UpdateValues(object value)
    {
        base.UpdateValues(value);
        dropdown.Current.Value = (T)value;
    }
}
