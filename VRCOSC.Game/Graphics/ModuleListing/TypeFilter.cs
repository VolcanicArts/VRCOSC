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
    private void load()
    {
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
                    new Dimension(GridSizeMode.Relative, 1f / 3f),
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
                            Text = "Group By:"
                        },
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(5),
                            Child = new VRCOSCDropdown<Group>
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                Items = Enum.GetValues<Group>()
                            }
                        }
                    }
                }
            }
        };
    }

    private enum Group
    {
        None,
        General,
        Health,
        Integrations,
        Random
    }

    private ModuleType? groupToType(Group group)
    {
        return group switch
        {
            Group.None => null,
            Group.General => ModuleType.General,
            Group.Health => ModuleType.Health,
            Group.Integrations => ModuleType.Integrations,
            Group.Random => ModuleType.Random,
            _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
        };
    }
}
