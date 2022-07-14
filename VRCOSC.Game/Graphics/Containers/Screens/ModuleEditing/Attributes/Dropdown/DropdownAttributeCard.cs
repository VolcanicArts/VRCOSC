// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Dropdown;

public class DropdownAttributeCard<T> : AttributeCard where T : Enum
{
    protected BasicDropdown<T> Dropdown = null!;

    public DropdownAttributeCard(ModuleAttributeData attributeData)
        : base(attributeData)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
            RelativeSizeAxes = Axes.Both,
            Size = new Vector2(1.0f, 0.5f),
            Padding = new MarginPadding(10),
            Child = Dropdown = new BasicDropdown<T>()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Items = Enum.GetValues(typeof(T)).Cast<T>()
            }
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
