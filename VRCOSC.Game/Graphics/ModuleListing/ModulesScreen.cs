// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.ModuleAttributes;
using VRCOSC.Game.Graphics.ModuleInfo;
using VRCOSC.Game.Graphics.RepoListing;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ModuleListing;

[Cached]
public sealed partial class ModulesScreen : BaseScreen
{
    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    private FillFlowContainer moduleFlow = null!;
    private RepoListingPopover repoListing = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        AddRange(new Drawable[]
        {
            new ModuleAttributesPopover(),
            new ModuleInfoPopover(),
            repoListing = new RepoListingPopover()
        });
    }

    public void ShowRepoListing()
    {
        repoListing.Show();
    }

    protected override BaseHeader CreateHeader() => new ModulesHeader();

    protected override Drawable CreateBody() => new VRCOSCScrollContainer
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        ClampExtension = 0,
        ScrollContent =
        {
            Child = moduleFlow = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                LayoutEasing = Easing.OutQuad,
                LayoutDuration = 150
            }
        }
    };

    protected override void LoadComplete()
    {
        gameManager.ModuleManager.ModuleCollections.Values.Select(collection => new DrawableModuleAssembly(collection)).ForEach(drawableModuleAssembly =>
        {
            moduleFlow.Add(drawableModuleAssembly);

            moduleFlow.Add(new LineSeparator
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre
            });
        });

        moduleFlow.Remove(moduleFlow.Last(), true);
    }
}
