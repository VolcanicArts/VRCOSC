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
        var value = attributeData.Attribute.Value;

        if (value.GetType().IsSubclassOf(typeof(Enum)))
        {
            Type instanceType = typeof(DropdownAttributeCard<>).MakeGenericType(value.GetType());
            return (Activator.CreateInstance(instanceType, attributeData) as Drawable)!;
        }

        if (attributeData is ModuleAttributeDataWithBounds attributeDataWithBounds)
        {
            return value switch
            {
                int => new IntSliderAttributeCard(attributeDataWithBounds),
                float => new FloatSliderAttributeCard(attributeDataWithBounds),
                _ => throw new ArgumentOutOfRangeException(nameof(ModuleAttributeDataWithBounds), "Cannot have bounds for a non-numeric value")
            };
        }

        return value switch
        {
            string => new TextAttributeCard(attributeData),
            int => new IntTextAttributeCard(attributeData),
            bool => new ToggleAttributeCard(attributeData),
            _ => throw new ArgumentOutOfRangeException(nameof(ModuleAttributeData), $"Type {value.GetType()} is not supported in the {nameof(AttributeFlow)}")
        };
    }
}
