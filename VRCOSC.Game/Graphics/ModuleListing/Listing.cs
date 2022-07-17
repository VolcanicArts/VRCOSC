// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class Listing : Container
{
    private readonly List<Module> modules = new();
    private FillFlowContainer<ModuleCard> moduleCardFlow = null!;

    [Resolved]
    private ModuleManager moduleManager { get; set; }

    [Resolved]
    private ModuleListingScreen moduleListingScreen { get; set; }

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

        moduleManager.Modules.ForEach(module => modules.Add(module));

        moduleListingScreen.SearchTermFilter.BindValueChanged(_ => sort());
        moduleListingScreen.TypeFilter.BindValueChanged(_ => sort());
        moduleListingScreen.SortFilter.BindValueChanged(_ => sort());

        sort();
    }

    private void sort()
    {
        moduleCardFlow.Clear();

        var sortType = moduleListingScreen.SortFilter.Value.SortType;
        var sortDirection = moduleListingScreen.SortFilter.Value.SortDirection;

        switch (sortType)
        {
            case SortType.Title:
                modules.Sort((m1, m2) => string.Compare(m1.Title, m2.Title, StringComparison.InvariantCultureIgnoreCase));
                break;

            case SortType.Type:
                modules.Sort((m1, m2) => m1.ModuleType.CompareTo(m2.ModuleType));
                break;

            case SortType.Author:
                modules.Sort((m1, m2) => string.Compare(m1.Author, m2.Author, StringComparison.InvariantCultureIgnoreCase));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(sortType), sortType, "Unknown sort type");
        }

        if (sortDirection.Equals(SortDirection.Descending) && !sortType.Equals(SortType.Type)) modules.Reverse();

        modules.ForEach(module => moduleCardFlow.Add(new ModuleCard(module)));

        filter();
    }

    private void filter()
    {
        var searchTerm = moduleListingScreen.SearchTermFilter.Value;
        var type = moduleListingScreen.TypeFilter.Value;

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
