// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.Containers.UI.Checkbox;

public class Checkbox : IconButton
{
    public BindableBool State { get; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        State.BindValueChanged(e =>
        {
            BackgroundColour.Value = e.NewValue ? VRCOSCColour.Green : VRCOSCColour.Red;
            Icon.Value = e.NewValue ? FontAwesome.Solid.Check : FontAwesome.Solid.Get(0xf00d);
        }, true);
    }

    protected override bool OnClick(ClickEvent e)
    {
        State.Toggle();
        return base.OnClick(e);
    }
}
