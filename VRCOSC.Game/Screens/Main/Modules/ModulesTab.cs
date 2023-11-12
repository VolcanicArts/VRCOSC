// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Modules;

public partial class ModulesTab : Container
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer assemblyFlowContainer;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY1
            },
            new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 0,
                ScrollContent =
                {
                    Child = assemblyFlowContainer = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Spacing = new Vector2(0, 10)
                    }
                }
            }
        };

        game.OnListingRefresh += Refresh;
        Refresh();
    }

    public void Refresh()
    {
        assemblyFlowContainer.Clear();

        appManager.ModuleManager.LocalModules.ForEach(pair =>
        {
            var title = pair.Key.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Unknown";
            assemblyFlowContainer.Add(new ModuleAssemblyContainer(title, pair.Value));
        });

        appManager.ModuleManager.RemoteModules.ForEach(pair =>
        {
            var title = pair.Key.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Unknown";
            assemblyFlowContainer.Add(new ModuleAssemblyContainer(title, pair.Value));
        });
    }
}
