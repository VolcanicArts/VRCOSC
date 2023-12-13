// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.SDK;

namespace VRCOSC.Screens.Main.Modules;

public partial class ModuleAssemblyContainer : FillFlowContainer
{
    private readonly string title;
    private readonly List<Module> modules;
    private readonly bool isLocal;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    [Cached]
    private Bindable<bool> collapsed = new();

    private FillFlowContainer collapseWrapper;
    private FillFlowContainer moduleFlow;

    public ModuleAssemblyContainer(string title, List<Module> modules, bool isLocal = false)
    {
        this.title = title;
        this.modules = modules;
        this.isLocal = isLocal;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Direction = FillDirection.Vertical;
        Masking = true;
        CornerRadius = 5;

        Children = new Drawable[]
        {
            new ModuleAssemblyContainerHeader(title, isLocal),
            collapseWrapper = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    moduleFlow = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical
                    },
                    new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        Colour = Colours.GRAY0
                    }
                }
            }
        };

        var even = false;
        modules.OrderBy(module => module.Type).ThenBy(module => module.SerialisedName).ForEach(module =>
        {
            moduleFlow.Add(new DrawableModule(module, even));
            even = !even;
        });

        collapsed.BindValueChanged(onCollapsedChange);
    }

    private void onCollapsedChange(ValueChangedEvent<bool> e)
    {
        collapseWrapper.ScaleTo(e.NewValue ? new Vector2(1, 0) : Vector2.One, 150, Easing.OutQuart);
    }
}
