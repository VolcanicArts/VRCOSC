// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Router;

public partial class RouterHeader : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Width = 0.8f;
        Masking = true;
        CornerRadius = 10;

        TextFlowContainer textFlow;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both,
            },
            textFlow = new TextFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                TextAnchor = Anchor.TopCentre
            }
        };

        textFlow.AddText("Router", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 40);
            t.Colour = ThemeManager.Current[ThemeAttribute.Text];
        });

        textFlow.AddParagraph("Here you can define port routing to route other programs through VRCOSC to VRChat and vice versa", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = ThemeManager.Current[ThemeAttribute.SubText];
        });
    }
}
