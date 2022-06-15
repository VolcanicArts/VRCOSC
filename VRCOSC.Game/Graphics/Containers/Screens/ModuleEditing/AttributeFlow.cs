// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
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
        FillFlowContainer<AttributeCard> attributeFlow;

        InternalChildren = new Drawable[]
        {
            new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Font = FrameworkFont.Regular.With(size: 50),
                Text = Title
            },
            attributeFlow = new FillFlowContainer<AttributeCard>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10)
            }
        };

        AttributesList.ForEach(attributeData =>
        {
            switch (Type.GetTypeCode(attributeData.Attribute.Value.GetType()))
            {
                case TypeCode.String:
                    attributeFlow.Add(new StringAttributeCard(attributeData));
                    break;

                case TypeCode.Int32:
                    attributeFlow.Add(new IntAttributeCard(attributeData));
                    break;

                case TypeCode.Boolean:
                    attributeFlow.Add(new BoolAttributeCard(attributeData));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        });
    }
}
