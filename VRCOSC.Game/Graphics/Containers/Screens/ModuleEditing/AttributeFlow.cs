// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Settings;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public abstract class AttributeFlow : FillFlowContainer
{
    [Resolved]
    private Bindable<Module> SourceModule { get; set; }

    protected virtual string Title => string.Empty;

    private FillFlowContainer<AttributeCard> attributeFlow;

    [BackgroundDependencyLoader]
    private void load()
    {
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

        SourceModule.BindValueChanged(_ =>
        {
            attributeFlow.Clear();

            if (SourceModule.Value == null) return;

            generateCards(SourceModule.Value);
        });
    }

    private void generateCards(Module source)
    {
        var attributeList = GetAttributeList(source);

        attributeList.ForEach(attributeData =>
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

    protected abstract List<ModuleAttributeData> GetAttributeList(Module source);
}
