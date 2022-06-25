// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing.Attributes;

public abstract class AttributeCard : Container
{
    private VRCOSCButton resetToDefault;

    protected readonly ModuleAttributeData AttributeData;

    protected AttributeCard(ModuleAttributeData attributeData)
    {
        AttributeData = attributeData;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        Height = 120;

        TextFlowContainer textFlow;

        InternalChildren = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreRight,
                Size = new Vector2(30, 50),
                Padding = new MarginPadding(5),
                Child = resetToDefault = new VRCOSCButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1.0f),
                    CornerRadius = 5,
                    CornerExponent = 2,
                    Alpha = AttributeData.Attribute.IsDefault ? 0 : 1,
                    Action = () => AttributeData.Attribute.SetDefault(),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = VRCOSCColour.BlueLight,
                        Radius = 5
                    }
                }
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = VRCOSCColour.Gray2
                    },
                    textFlow = new TextFlowContainer
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10)
                    },
                }
            }
        };

        textFlow.AddText(AttributeData.DisplayName, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
        });
        textFlow.AddParagraph(AttributeData.Description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = VRCOSCColour.Gray9;
        });

        AttributeData.Attribute.ValueChanged += updateResetToDefault;
    }

    protected override void LoadComplete()
    {
        AttributeData.Attribute.TriggerChange();
    }

    private void updateResetToDefault(ValueChangedEvent<object> _)
    {
        if (!AttributeData.Attribute.IsDefault)
            resetToDefault.Show();
        else
            resetToDefault.Hide();
    }

    protected override void Dispose(bool isDisposing)
    {
        AttributeData.Attribute.ValueChanged -= updateResetToDefault;
        base.Dispose(isDisposing);
    }
}
