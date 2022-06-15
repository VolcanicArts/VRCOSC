// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Button;
using VRCOSC.Game.Graphics.Drawables.Triangles;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public abstract class AttributeCard : Container
{
    protected VRCOSCButton ResetToDefault;

    protected readonly ModuleAttributeData attributeData;

    protected AttributeCard(ModuleAttributeData attributeData)
    {
        this.attributeData = attributeData;
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
                Child = ResetToDefault = new VRCOSCButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1.0f),
                    CornerRadius = 5,
                    CornerExponent = 2,
                    Alpha = attributeData.Attribute.IsDefault ? 0 : 1,
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
                EdgeEffect = VRCOSCEdgeEffects.BasicShadow,
                CornerRadius = 10,
                BorderThickness = 3,
                Children = new Drawable[]
                {
                    new TrianglesBackground
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        ColourLight = VRCOSCColour.Gray7,
                        ColourDark = VRCOSCColour.Gray7.Darken(0.25f),
                        Velocity = 0.5f,
                        TriangleScale = 3
                    },
                    new Box
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.5f, 1),
                        Colour = ColourInfo.GradientHorizontal(Colour4.Black.Opacity(0.75f), VRCOSCColour.Invisible)
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

        textFlow.AddText(attributeData.DisplayName, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
        });
        textFlow.AddParagraph(attributeData.Description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = VRCOSCColour.Gray9;
        });

        ResetToDefault.Action += () => attributeData.Attribute.SetDefault();

        attributeData.Attribute.ValueChanged += updateResetToDefault;
    }

    private void updateResetToDefault(ValueChangedEvent<object> _)
    {
        if (!attributeData.Attribute.IsDefault)
            ResetToDefault.Show();
        else
            ResetToDefault.Hide();
    }

    protected override void Dispose(bool isDisposing)
    {
        attributeData.Attribute.ValueChanged -= updateResetToDefault;
        base.Dispose(isDisposing);
    }
}
