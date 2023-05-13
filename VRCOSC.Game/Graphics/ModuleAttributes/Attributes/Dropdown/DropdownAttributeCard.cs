// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Dropdown;

public sealed partial class DropdownAttributeCard<T> : AttributeCard where T : Enum
{
    private VRCOSCDropdown<T> dropdown = null!;

    public DropdownAttributeCard(ModuleAttribute attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(dropdown = new VRCOSCDropdown<T>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues(typeof(T)).Cast<T>(),
            Current = { Value = (T)AttributeData.Attribute.Value }
        });
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        dropdown.Current.ValueChanged += e => UpdateAttribute(e.NewValue);
    }

    protected override void SetDefault()
    {
        base.SetDefault();
        dropdown.Current.Value = (T)AttributeData.Attribute.Value;
    }
}
