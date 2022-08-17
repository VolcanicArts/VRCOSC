// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
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

public sealed class AttributeFlow : FillFlowContainer
{
    public BindableList<ModuleAttribute> AttributesList = new();

    private readonly string title;

    private FillFlowContainer<AttributeCard> attributeCardFlow = null!;

    public AttributeFlow(string title)
    {
        this.title = title;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Direction = FillDirection.Vertical;
        Spacing = new Vector2(0, 10);
        Padding = new MarginPadding(10);

        Children = new Drawable[]
        {
            new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = FrameworkFont.Regular.With(size: 50),
                Text = title
            },
            attributeCardFlow = new FillFlowContainer<AttributeCard>
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

    public new void Clear()
    {
        attributeCardFlow.Clear();
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        AttributesList.BindCollectionChanged((_, _) =>
        {
            attributeCardFlow.Clear();
            AttributesList.ForEach(attributeData => attributeCardFlow.Add(generateCard(attributeData)));
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

        if (attributeData is ModuleAttributeSingleWithBounds attributeDataWithBounds)
        {
            return value switch
            {
                int => new IntSliderAttributeCard(attributeDataWithBounds),
                float => new FloatSliderAttributeCard(attributeDataWithBounds),
                _ => throw new ArgumentOutOfRangeException(nameof(attributeDataWithBounds), "Cannot have bounds for a non-numeric value")
            };
        }

        return value switch
        {
            string => new TextAttributeCard(attributeData),
            int => new IntTextAttributeCard(attributeData),
            bool => new ToggleAttributeCard(attributeData),
            _ => throw new ArgumentOutOfRangeException(nameof(attributeData), $"Type {value.GetType()} is not supported in the {nameof(AttributeFlow)}")
        };
    }
}
