// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Slider;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Text;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes.Toggle;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public sealed class AttributeFlow : FillFlowContainer
{
    public string Title { get; init; }
    public List<ModuleAttributeData> AttributesList { get; init; }

    public AttributeFlow()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Direction = FillDirection.Vertical;
        Spacing = new Vector2(0, 10);
        Padding = new MarginPadding(10);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new SpriteText
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Font = FrameworkFont.Regular.With(size: 50),
            Text = Title
        });

        AttributesList.ForEach(attributeData =>
        {
            switch (Type.GetTypeCode(attributeData.Attribute.Value.GetType()))
            {
                case TypeCode.String:
                    Add(new TextAttributeCard(attributeData));
                    break;

                case TypeCode.Int32:
                    if (attributeData is ModuleAttributeDataWithBounds attributeDataWithBounds)
                    {
                        Add(new IntSliderAttributeCard(attributeDataWithBounds));
                    }
                    else
                    {
                        Add(new IntTextAttributeCard(attributeData));
                    }

                    break;

                case TypeCode.Single:
                    Add(new FloatSliderAttributeCard((ModuleAttributeDataWithBounds)attributeData));
                    break;

                case TypeCode.Boolean:
                    Add(new ToggleAttributeCard(attributeData));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }
}
