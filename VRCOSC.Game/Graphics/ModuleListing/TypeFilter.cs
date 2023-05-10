// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleListing;

public sealed partial class TypeFilter : Container
{
    [Resolved]
    private Bindable<Module.ModuleType?> typeFilter { get; set; } = null!;

    private readonly VRCOSCDropdown<Group> dropdown;

    public TypeFilter()
    {
        RelativeSizeAxes = Axes.Both;

        Child = dropdown = new VRCOSCDropdown<Group>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues<Group>(),
            Current = { Value = Group.All }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        dropdown.Current.BindValueChanged(group => typeFilter.Value = groupToType(group.NewValue), true);
    }

    private enum Group
    {
        All = -1,
        Health = 0,
        Accessibility = 1,
        OpenVR = 2,
        Integrations = 3,
        General = 4
    }

    private static Module.ModuleType? groupToType(Group group) => group == Group.All ? null : (Module.ModuleType)(int)group;
}
