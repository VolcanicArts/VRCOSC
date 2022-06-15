// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class AttributeFlow : FillFlowContainer
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

            GenerateCards(SourceModule.Value);
        });
    }

    protected virtual void GenerateCards(Module source) { }

    protected void AddAttributeCard(AttributeCard card)
    {
        attributeFlow.Add(card);
    }
}
