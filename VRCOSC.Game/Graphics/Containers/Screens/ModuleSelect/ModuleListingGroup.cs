// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleListingGroup : FillFlowContainer, IFilterable
{
    public readonly ModuleType ModuleType;
    public readonly List<Module> Modules;
    public readonly BindableBool State = new(true);

    public ModuleListingGroup(ModuleType moduleType, List<Module> modules)
    {
        ModuleType = moduleType;
        Modules = modules;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        List<LocalisableString> localFilters = new List<LocalisableString>();

        modules.ForEach(module =>
        {
            localFilters.Add(module.Title);
            localFilters.Add(module.Author);
            module.Tags.ForEach(tag => localFilters.Add(tag));
        });

        FilterTerms = localFilters;
    }

    [BackgroundDependencyLoader]
    private void load(ModuleSelection moduleSelection)
    {
        SearchContainer<ModuleCard> moduleCardFlow;

        Children = new Drawable[]
        {
            new ModuleListingGroupDropdownHeader
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                Height = 50,
                Masking = true,
                CornerRadius = 10,
                State = (BindableBool)State.GetBoundCopy(),
                Title = ModuleType.ToString()
            },
            new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding
                {
                    Horizontal = 10
                },
                Child = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = 500,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = VRCOSCColour.Gray3
                        },
                        moduleCardFlow = new SearchContainer<ModuleCard>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(10),
                            Spacing = new Vector2(10)
                        }
                    }
                }
            }
        };

        Modules.ForEach(module => moduleCardFlow.Add(new ModuleCard(module)));
        moduleSelection.SearchString.ValueChanged += searchTerm =>
        {
            moduleCardFlow.SearchTerm = searchTerm.NewValue;
            if (!State.Value) State.Value = true;
        };

        State.BindValueChanged(e => moduleCardFlow.FadeTo(e.NewValue ? 1 : 0, 100), true);
    }

    public IEnumerable<LocalisableString> FilterTerms { get; private set; }

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
