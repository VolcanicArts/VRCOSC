// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Game.Graphics.ModuleAttributes;

public partial class ModuleAttributeFlow : Container
{
    private readonly string attributeName;
    public readonly BindableList<ModuleAttribute> AttributeList = new();

    private FillFlowContainer attributeFlow = null!;
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
                Child = new VRCOSCScrollContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    ClampExtension = 0,
                    ScrollContent =
                    {
                        Child = attributeFlow = new FillFlowContainer
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

            foreach (ModuleAttribute moduleAttribute in e.NewItems)
            {
                attributeFlow.Add(moduleAttribute.GetAssociatedCard());
            }

            noAttributesContainer.Alpha = attributeFlow.Any() ? 0 : 1;
        }, true);
    }
}
