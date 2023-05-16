// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ModuleAttributes;

public partial class ModuleAttributeFlowContainer : Container
{
    private readonly string attributeName;

    public ModuleAttributeFlow AttributeFlow = null!;

    public ModuleAttributeFlowContainer(string attributeName)
    {
        this.attributeName = attributeName;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        TextFlowContainer titleFlow;

        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable[]
                {
                    titleFlow = new TextFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TextAnchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    }
                },
                null,
                new Drawable[]
                {
                    AttributeFlow = new ModuleAttributeFlow(attributeName)
                }
            }
        };

        titleFlow.AddText($"{attributeName}s", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });
    }
}
