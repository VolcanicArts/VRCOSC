// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.Containers.UI.Button;

public class DropdownButton : IconButton
{
    public BindableBool State { get; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Icon.Value = FontAwesome.Solid.ArrowCircleRight;
        BackgroundColour.Value = VRCOSCColour.Invisible;
        State.BindValueChanged(e =>
        {
            var targetRotation = e.NewValue ? 90 : 0;
            spriteIcon.RotateTo(targetRotation, 400, Easing.OutElastic);
        }, true);
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (Enabled.Value) State.Toggle();
        return base.OnClick(e);
    }
}
