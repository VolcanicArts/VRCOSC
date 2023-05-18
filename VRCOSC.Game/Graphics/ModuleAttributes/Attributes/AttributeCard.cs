// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes;

public abstract partial class AttributeCard<T> : Container where T : ModuleAttribute
{
    private readonly VRCOSCButton resetToDefault;

    protected override FillFlowContainer Content { get; }

    protected readonly T AttributeData;

    protected override bool ShouldBeConsideredForInput(Drawable child) => AttributeData.Enabled;

    protected AttributeCard(T attributeData)
    {
        AttributeData = attributeData;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        TextFlowContainer textFlow;

        InternalChildren = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Size = new Vector2(35),
                Padding = new MarginPadding(5),
                Depth = float.MinValue,
                Child = resetToDefault = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Action = setDefault,
                    BorderThickness = 2,
                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                    Icon = FontAwesome.Solid.Undo,
                    IconPadding = 7,
                    IconShadow = true,
                    Circular = true
                }
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 10,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                BorderThickness = 2,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = ThemeManager.Current[ThemeAttribute.Light]
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            textFlow = new TextFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                AutoSizeDuration = 150,
                                AutoSizeEasing = Easing.OutQuint
                            },
                            Content = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 10),
                                AutoSizeDuration = 150,
                                AutoSizeEasing = Easing.OutQuint
                            }
                        }
                    }
                }
            }
        };

        textFlow.AddText(AttributeData.Name, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 25);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        textFlow.AddParagraph(AttributeData.Description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });
    }

    protected override void Update()
    {
        resetToDefault.FadeTo(AttributeData.IsDefault() ? 0 : 1, 200, Easing.OutQuart);
        this.FadeTo(AttributeData.Enabled ? 1 : 0.5f, 150, Easing.OutQuart);
    }

    private void setDefault()
    {
        AttributeData.SetDefault();
        OnSetDefault();
    }

    protected virtual void OnSetDefault() { }
}
