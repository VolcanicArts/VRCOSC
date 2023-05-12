// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed partial class Listing : Container
{
    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    [Resolved]
    private Bindable<Module.ModuleType?> typeFilter { get; set; } = null!;

    private readonly FillFlowContainer<ModuleCard> moduleCardFlow;

    public Listing()
    {
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding(5);

        Children = new Drawable[]
        {
            new VRCOSCScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = true,
                ClampExtension = 0,
                Child = moduleCardFlow = new FillFlowContainer<ModuleCard>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Right = 12 },
                    Spacing = new Vector2(0, 5)
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        gameManager.ModuleManager.ForEach(module => moduleCardFlow.Add(new ModuleCard(module)));
    }

    protected override void LoadComplete()
    {
        typeFilter.BindValueChanged(_ => filter(), true);
    }

    private void filter()
    {
        var type = typeFilter.Value;

        moduleCardFlow.ForEach(moduleCard =>
        {
            if (type is null || moduleCard.Module.Type.Equals(type))
                moduleCard.Show();
            else
                moduleCard.Hide();
        });
    }
}
