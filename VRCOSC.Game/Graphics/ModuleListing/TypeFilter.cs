// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed class TypeFilter : Container
{
    public TypeFilter()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load(ModuleListingScreen moduleListingScreen)
    {
        VRCOSCDropdown<Group> dropdown;
        Children = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                Child = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray3
                }
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.2f),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new TextFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Text = "Group By:"
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Child = dropdown = new VRCOSCDropdown<Group>
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                Items = Enum.GetValues<Group>()
                            }
                        }
                    }
                }
            }
        };

        dropdown.Current.BindValueChanged(group => moduleListingScreen.TypeFilter.Value = groupToType(group.NewValue));
    }

    private enum Group
    {
        All,
        General,
        Health,
        Integrations,
        Random
    }

    private static ModuleType? groupToType(Group group)
    {
        return group switch
        {
            Group.All => null,
            Group.General => ModuleType.General,
            Group.Health => ModuleType.Health,
            Group.Integrations => ModuleType.Integrations,
            Group.Random => ModuleType.Random,
            _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
        };
    }
}
