// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleEditing.Attributes;

public abstract partial class AttributeCard : Container
{
    private VRCOSCButton resetToDefault = null!;
    protected FillFlowContainer ContentFlow = null!;
    protected FillFlowContainer LayoutFlow = null!;

    public readonly ModuleAttribute AttributeData;
    public bool Enable { get; set; } = true;

    protected override bool ShouldBeConsideredForInput(Drawable child) => Enable;

    protected AttributeCard(ModuleAttribute attributeData)
    {
        AttributeData = attributeData;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        TextFlowContainer textFlow;

        InternalChildren = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreRight,
                Size = new Vector2(30, 60),
                Padding = new MarginPadding(5),
                Child = resetToDefault = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Action = SetDefault,
                    IconPadding = 4,
                    CornerRadius = 10,
                    BorderThickness = 2,
                    BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                    BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                    Icon = FontAwesome.Solid.Undo,
                    IconShadow = true
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
                        Colour = ThemeManager.Current[ThemeAttribute.Darker]
                    },
                    LayoutFlow = new FillFlowContainer
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
                                TextAnchor = Anchor.TopLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            },
                            ContentFlow = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 10)
                            }
                        }
                    }
                }
            }
        };
        textFlow.AddText(AttributeData.Metadata.DisplayName, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });
        textFlow.AddParagraph(AttributeData.Metadata.Description, t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });
    }

    protected virtual void SetDefault()
    {
        AttributeData.SetDefault();
    }

    protected void UpdateResetToDefault(bool show)
    {
        var newAlpha = show ? 1 : 0;
        if (Math.Abs(resetToDefault.Alpha - newAlpha) < 0.01f) return;

        resetToDefault.FadeTo(newAlpha, 200, Easing.OutQuart);
    }

    #region Graphics

    protected static VRCOSCTextBox CreateTextBox()
    {
        return new VRCOSCTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 40,
            Masking = true,
            CornerRadius = 5,
            BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            BorderThickness = 2
        };
    }

    #endregion
}
