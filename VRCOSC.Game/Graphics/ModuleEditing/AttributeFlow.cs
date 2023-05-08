// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Dropdown;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Slider;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Text;
using VRCOSC.Game.Graphics.ModuleEditing.Attributes.Toggle;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing;

public sealed partial class AttributeFlow : FillFlowContainer<AttributeCard>
{
    public BindableList<ModuleAttribute> AttributesList = new();

    protected override FillFlowContainer<AttributeCard> Content { get; }

    public AttributeFlow()
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
            checkShouldDisplay();
        }, true);
    }

    private void checkShouldDisplay()
    {
        this.ForEach(card =>
        {
            card.Enable = card.AttributeData.Enabled;
            card.FadeTo(card.AttributeData.Enabled ? 1 : 0.25f, 250, Easing.OutQuad);
        });
    }

    private AttributeCard generateCard(ModuleAttribute attributeData)
    {
        var value = attributeData.Attribute.Value;

        if (value.GetType().IsSubclassOf(typeof(Enum)))
        {
            attributeData.Attribute.BindValueChanged(_ => checkShouldDisplay());
            Type instanceType = typeof(DropdownAttributeCard<>).MakeGenericType(value.GetType());
            return (Activator.CreateInstance(instanceType, attributeData) as AttributeCard)!;
        }

        switch (attributeData)
        {
            case ModuleAttributeWithButton attributeSingleWithButton:
                switch (value)
                {
                    case string:
                        return new ButtonStringAttributeCard(attributeSingleWithButton);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(attributeSingleWithButton), "Cannot generate button with non-text counterpart");
                }

            case ModuleAttributeWithBounds attributeDataWithBounds:
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
                        return new TextAttributeCard<StringTextBox, string>(attributeData);

                    case int:
                        return new TextAttributeCard<IntTextBox, int>(attributeData);

                    case bool:
                        attributeData.Attribute.BindValueChanged(_ => checkShouldDisplay());
                        return new ToggleAttributeCard(attributeData);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(attributeData), $"Type {value.GetType()} is not supported in the {nameof(AttributeFlow)}");
                }
        }
    }
}
