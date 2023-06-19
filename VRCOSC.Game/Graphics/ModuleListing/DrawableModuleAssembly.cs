// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public partial class DrawableModuleAssembly : Container
{
    private readonly ModuleCollection moduleCollection;
    private FillFlowContainer moduleFlow = null!;

    public DrawableModuleAssembly(ModuleCollection moduleCollection)
    {
        this.moduleCollection = moduleCollection;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        Children = new Drawable[]
        {
            moduleFlow = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(0, 5),
                LayoutEasing = Easing.OutQuad,
                LayoutDuration = 150,
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = FrameworkFont.Regular.With(size: 30),
                        Text = moduleCollection.Title,
                        Colour = ThemeManager.Current[ThemeAttribute.Text]
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        moduleFlow.AddRange(moduleCollection.Modules.Select(module => new ModuleCard(module)));
    }
}
