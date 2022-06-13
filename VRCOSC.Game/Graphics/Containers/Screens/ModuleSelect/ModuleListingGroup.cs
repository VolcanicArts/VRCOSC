// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleListingGroup : Container, IFilterable
{
    private readonly ModuleGroup moduleGroup;

    [Resolved]
    private ModuleSelection moduleSelection { get; set; }

    [Resolved]
    private ModuleManager moduleManager { get; set; }

    public ModuleListingGroup(ModuleGroup moduleGroup)
    {
        this.moduleGroup = moduleGroup;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        populateFilter();
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        SearchContainer<ModuleCard> moduleCardFlow;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Aqua
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = moduleCardFlow = new SearchContainer<ModuleCard>
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical
                }
            }
        };

        moduleGroup.ForEach(moduleContainer => moduleCardFlow.Add(new ModuleCard(moduleContainer.Module)));
        moduleSelection.SearchString.ValueChanged += (searchTerm) => moduleCardFlow.SearchTerm = searchTerm.NewValue;
    }

    private void populateFilter()
    {
        List<LocalisableString> localFilters = new List<LocalisableString>();

        localFilters.Add(moduleGroup.Type.ToString());

        moduleGroup.ForEach(module =>
        {
            localFilters.Add(module.Module.Title);
            localFilters.Add(module.Module.Author);
            module.Module.Tags.ForEach(tag => localFilters.Add(tag));
        });

        FilterTerms = localFilters;
    }

    public IEnumerable<LocalisableString> FilterTerms { get; set; }

    public bool MatchingFilter
    {
        set
        {
            if (value)
                this.FadeIn();
            else
                this.FadeOut();
        }
    }

    public bool FilteringActive { get; set; }
}
