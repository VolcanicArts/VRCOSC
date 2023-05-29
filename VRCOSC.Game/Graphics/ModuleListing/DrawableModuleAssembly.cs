// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using Module = VRCOSC.Game.Modules.Module;

namespace VRCOSC.Game.Graphics.ModuleListing;

public partial class DrawableModuleAssembly : Container
{
    private readonly Assembly moduleAssembly;
    private readonly List<Module> modules;
    private FillFlowContainer moduleFlow = null!;

    public DrawableModuleAssembly(Assembly moduleAssembly, List<Module> modules)
    {
        this.moduleAssembly = moduleAssembly;
        this.modules = modules;
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
                        Text = moduleAssembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "UNKNOWN"
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        moduleFlow.AddRange(modules.Select(module => new ModuleCard(module)));
    }
}
