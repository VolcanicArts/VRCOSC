// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Dropdown;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Slider;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Toggle;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed class AttributeFlow : FillFlowContainer<AttributeCard>
{
    public BindableList<ModuleAttribute> AttributesList = new();

    protected override FillFlowContainer<AttributeCard> Content { get; }

    public AttributeFlow(string title)
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Direction = FillDirection.Vertical;
        Spacing = new Vector2(0, 10);
        Padding = new MarginPadding(10);

        InternalChildren = new Drawable[]
        {
            new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = FrameworkFont.Regular.With(size: 50),
                Text = title
            },
            Content = new FillFlowContainer<AttributeCard>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10)
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        AttributesList.BindCollectionChanged((_, _) =>
        {
            Clear();
            AttributesList.ForEach(attributeData => Add(generateCard(attributeData)));
        }, true);
    }

    private static AttributeCard generateCard(ModuleAttribute attributeData)
    {
        return attributeData switch
        {
            ModuleAttributeList attributeDataList => generateListCard(attributeDataList),
            ModuleAttributeSingle attributeDataSingle => generateSingleCard(attributeDataSingle),
            _ => throw new ArgumentOutOfRangeException(nameof(attributeData), "No suitable card type was found for the specified ModuleAttribute")
        };
    }

    private static AttributeCard generateListCard(ModuleAttributeList attributeData)
    {
        if (attributeData.Type == typeof(int))
            return new IntTextAttributeCardList(attributeData);

        if (attributeData.Type == typeof(string))
            return new TextAttributeCardList(attributeData);

        throw new ArgumentOutOfRangeException(nameof(attributeData), "Cannot generate lists for non-text values");
    }

    private static AttributeCard generateSingleCard(ModuleAttributeSingle attributeData)
    {
        var value = attributeData.Attribute.Value;

        if (value.GetType().IsSubclassOf(typeof(Enum)))
        {
            Type instanceType = typeof(DropdownAttributeCard<>).MakeGenericType(value.GetType());
            return (Activator.CreateInstance(instanceType, attributeData) as AttributeCard)!;
        }

        switch (attributeData)
        {
            case ModuleAttributeSingleWithButton attributeSingleWithButton:
                switch (value)
                {
                    case string:
                        return new ButtonTextAttributeCard(attributeSingleWithButton);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(attributeSingleWithButton), "Cannot generate button with non-text counterpart");
                }

            case ModuleAttributeSingleWithBounds attributeDataWithBounds:
                switch (value)
                {
                    case int:
                        return new IntSliderAttributeCard(attributeDataWithBounds);

                    case float:
                        return new FloatSliderAttributeCard(attributeDataWithBounds);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(attributeDataWithBounds), "Cannot have bounds for a non-numeric value");
                }

            default:
                switch (value)
                {
                    case string:
                        return new TextAttributeCard(attributeData);

                    case int:
                        return new IntTextAttributeCard(attributeData);

                    case bool:
                        return new ToggleAttributeCard(attributeData);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(attributeData), $"Type {value.GetType()} is not supported in the {nameof(AttributeFlow)}");
                }
        }
    }
}
