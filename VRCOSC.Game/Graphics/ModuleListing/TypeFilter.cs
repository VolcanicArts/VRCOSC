// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.UI;
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
    private void load(VRCOSCGame game)
    {
        VRCOSCDropdown<Group> dropdown;
        Child = dropdown = new VRCOSCDropdown<Group>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues<Group>()
        };

        dropdown.Current.BindValueChanged(group => game.TypeFilter.Value = groupToType(group.NewValue));
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
