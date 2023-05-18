// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Dropdown;

public sealed partial class DropdownAttributeCard<T> : AttributeCard<ModuleEnumAttribute<T>> where T : Enum
{
    public DropdownAttributeCard(ModuleEnumAttribute<T> attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new VRCOSCDropdown<T>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues(typeof(T)).Cast<T>(),
            Current = AttributeData.Attribute.GetBoundCopy()
        });
    }
}
