// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
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

    private FillFlowContainer assemblyFlowContainer = null!;
    private TextFlowContainer noModulesText = null!;

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
            },
            noModulesText = new TextFlowContainer(t => { t.Font = Fonts.REGULAR.With(size: 40); })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TextAnchor = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Text = "You have no modules!\nInstall some using the package manager"
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
            assemblyFlowContainer.Add(new ModuleAssemblyContainer(title, pair.Value, true));
        });

        appManager.ModuleManager.RemoteModules.ForEach(pair =>
        {
            var title = pair.Key.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Unknown";
            assemblyFlowContainer.Add(new ModuleAssemblyContainer(title, pair.Value));
        });

        noModulesText.Alpha = assemblyFlowContainer.Any() ? 0 : 1;
    }
}
