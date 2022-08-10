// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Dropdown;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Slider;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Toggle;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed class AttributeFlow : FillFlowContainer
{
    public List<ModuleAttributeData> AttributesList { get; init; } = null!;

    public AttributeFlow(string title)
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Direction = FillDirection.Vertical;
        Spacing = new Vector2(0, 10);
        Padding = new MarginPadding(10);

        Add(new SpriteText
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Font = FrameworkFont.Regular.With(size: 50),
            Text = title
        });
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AttributesList.ForEach(attributeData => Add(generateCard(attributeData)));
    }

    private Drawable generateCard(ModuleAttributeData attributeData)
    {
        var valueType = attributeData.Attribute.Value.GetType();

        if (valueType.IsSubclassOf(typeof(Enum)))
        {
            Type instanceType = typeof(DropdownAttributeCard<>).MakeGenericType(valueType);
            return (Activator.CreateInstance(instanceType, attributeData) as Drawable)!;
        }

        if (attributeData is ModuleAttributeDataWithBounds attributeDataWithBounds)
        {
            if (valueType == typeof(int))
                return new IntSliderAttributeCard(attributeDataWithBounds);
            if (valueType == typeof(float))
                return new FloatSliderAttributeCard(attributeDataWithBounds);
        }

        if (valueType == typeof(string))
            return new TextAttributeCard(attributeData);

        if (valueType == typeof(int))
            return new IntTextAttributeCard(attributeData);

        if (valueType == typeof(bool))
            return new ToggleAttributeCard(attributeData);

        throw new ArgumentOutOfRangeException(nameof(ModuleAttributeData), $"Unknown {nameof(ModuleAttributeData)} type");
    }
}
