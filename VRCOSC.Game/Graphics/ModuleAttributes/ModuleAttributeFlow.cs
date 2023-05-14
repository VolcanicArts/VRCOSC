// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Dropdown;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Slider;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Text;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Toggle;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes;

public partial class ModuleAttributeFlow : Container
{
    private readonly string attributeName;
    public readonly BindableList<ModuleAttribute> AttributeList = new();

    private FillFlowContainer<AttributeCard> attributeFlow = null!;
    private TextFlowContainer noAttributesContainer = null!;

    public ModuleAttributeFlow(string attributeName)
    {
        this.attributeName = attributeName;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            noAttributesContainer = new TextFlowContainer(t =>
            {
                t.Font = FrameworkFont.Regular.With(size: 40);
                t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextAnchor = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Text = $"No {attributeName.ToLowerInvariant()}s\navailable",
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(2),
                Child = new BasicScrollContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    ClampExtension = 0,
                    ScrollbarVisible = false,
                    ScrollContent =
                    {
                        Child = attributeFlow = new FillFlowContainer<AttributeCard>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(10),
                            Spacing = new Vector2(5, 5),
                            Direction = FillDirection.Full,
                            LayoutEasing = Easing.OutQuad,
                            LayoutDuration = 150
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        AttributeList.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is null) return;

            attributeFlow.Clear();

            foreach (ModuleAttribute newAttribute in e.NewItems)
            {
                attributeFlow.Add(generateCard(newAttribute));
            }

            noAttributesContainer.Alpha = attributeFlow.Any() ? 0 : 1;

            checkShouldDisplay();
        }, true);
    }

    private void checkShouldDisplay()
    {
        attributeFlow.ForEach(card =>
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
                        throw new ArgumentOutOfRangeException(nameof(attributeData), $"Type {value.GetType()} is not supported in the {nameof(ModuleAttributeFlow)}");
                }
        }
    }
}
