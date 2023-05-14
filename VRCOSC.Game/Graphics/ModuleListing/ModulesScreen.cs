// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.ModuleAttributes;
using VRCOSC.Game.Graphics.ModuleInfo;
using VRCOSC.Game.Graphics.Screen;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ModuleListing;

[Cached]
public sealed partial class ModulesScreen : BaseScreen
{
    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    private FillFlowContainer<ModuleCard> moduleCardFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        AddRange(new Drawable[]
        {
            new ModuleAttributesPopover(),
            new ModuleInfoPopover()
        });
    }

    protected override BaseHeader CreateHeader() => new ModulesHeader();

    protected override Drawable CreateBody() => new BasicScrollContainer
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both,
        ClampExtension = 0,
        ScrollbarVisible = false,
        ScrollContent =
        {
            Child = moduleCardFlow = new FillFlowContainer<ModuleCard>
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Direction = FillDirection.Full,
                Spacing = new Vector2(0, 5),
                LayoutEasing = Easing.OutQuad,
                LayoutDuration = 150
            }
        }
    };

    protected override void LoadComplete()
    {
        gameManager.ModuleManager.ForEach(module => moduleCardFlow.Add(new ModuleCard(module)));
    }
}
