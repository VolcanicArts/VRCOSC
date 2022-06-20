// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using VRCOSC.Game.Graphics.Containers.UI.Dynamic;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleListingGroup : Container, IFilterable
{
    private readonly ModuleGroup moduleGroup;

    private SearchContainer<ModuleCard> moduleCardFlow;
    private DropdownButton dropdownButton;

    public ModuleListingGroup(ModuleGroup moduleGroup)
    {
        this.moduleGroup = moduleGroup;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Padding = new MarginPadding(5);

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
        Children = new Drawable[]
        {
            new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(5),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Colour = VRCOSCColour.Gray5
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    dropdownButton = new DropdownButton
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        FillMode = FillMode.Fit,
                                        State = { Value = true }
                                    },
                                    new SpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Text = moduleGroup.Type.ToString(),
                                        Font = FrameworkFont.Regular.With(size: 30)
                                    }
                                }
                            }
                        }
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
                                    AutoSizeAxes = Axes.Y
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
            if (!dropdownButton.State.Value) dropdownButton.State.Toggle();
        };

        dropdownButton.State.ValueChanged += e => moduleCardFlow.FadeTo(e.NewValue ? 1 : 0, 100);
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
