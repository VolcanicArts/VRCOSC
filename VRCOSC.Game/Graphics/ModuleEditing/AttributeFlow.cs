// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
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
    public BindableList<ModuleAttributeData> AttributesList = new();

    private readonly string title;

    public AttributeFlow(string title)
    {
        this.title = title;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Direction = FillDirection.Vertical;
        Spacing = new Vector2(0, 10);
        Padding = new MarginPadding(10);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        AttributesList.BindCollectionChanged((_, _) =>
        {
            Clear();

            if (AttributesList.Count == 0) return;

            Add(new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = FrameworkFont.Regular.With(size: 50),
                Text = title
            });

            AttributesList.ForEach(attributeData => Add(generateCard(attributeData)));
        }, true);
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
