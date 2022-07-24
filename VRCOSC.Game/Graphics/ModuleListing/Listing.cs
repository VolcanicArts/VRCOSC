// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class Listing : Container
{
    private FillFlowContainer<ModuleCard> moduleCardFlow = null!;

    [Resolved]
    private ModuleManager moduleManager { get; set; }

    [Resolved]
    private VRCOSCGame game { get; set; }

    public Listing()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding
        {
            Horizontal = 5,
            Top = 2.5f,
            Bottom = 5
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new VRCOSCScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
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
                    Padding = new MarginPadding
                    {
                        Right = 12
                    },
                    Spacing = new Vector2(0, 5)
                }
            }
        };

        moduleManager.Modules.ForEach(module => moduleCardFlow.Add(new ModuleCard(module)));

        game.SearchTermFilter.BindValueChanged(_ => filter());
        game.TypeFilter.BindValueChanged(_ => filter());

        filter();
    }

    private void filter()
    {
        var searchTerm = game.SearchTermFilter.Value;
        var type = game.TypeFilter.Value;

        moduleCardFlow.ForEach(moduleCard =>
        {
            var hasValidTitle = moduleCard.Module.Title.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase);
            var hasValidType = type == null || moduleCard.Module.ModuleType.Equals(type);

            if (hasValidTitle && hasValidType)
                moduleCard.Show();
            else
                moduleCard.Hide();
        });
    }
}
