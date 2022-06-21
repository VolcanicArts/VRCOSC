// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleListingGroup : Container, IFilterable
{
    private readonly ModuleGroup moduleGroup;

    private readonly BindableBool state = new(true);

    public ModuleListingGroup(ModuleGroup moduleGroup)
    {
        this.moduleGroup = moduleGroup;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        List<LocalisableString> localFilters = new List<LocalisableString>();

        moduleGroup.ForEach(module =>
        {
            localFilters.Add(module.Module.Title);
            localFilters.Add(module.Module.Author);
            module.Module.Tags.ForEach(tag => localFilters.Add(tag));
        });

        FilterTerms = localFilters;
    }

    [BackgroundDependencyLoader]
    private void load(ModuleSelection moduleSelection)
    {
        SearchContainer<ModuleCard> moduleCardFlow;

        Children = new Drawable[]
        {
            new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
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
                        State = (BindableBool)state.GetBoundCopy(),
                        Title = moduleGroup.Type.ToString()
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
                }
            }
        };

        moduleGroup.ForEach(moduleContainer => moduleCardFlow.Add(new ModuleCard(moduleContainer.Module)));
        moduleSelection.SearchString.ValueChanged += searchTerm =>
        {
            moduleCardFlow.SearchTerm = searchTerm.NewValue;
            if (!state.Value) state.Value = true;
        };

        state.BindValueChanged(e => moduleCardFlow.FadeTo(e.NewValue ? 1 : 0, 100), true);
    }

    protected override bool OnClick(ClickEvent e)
    {
        state.Toggle();
        return true;
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
