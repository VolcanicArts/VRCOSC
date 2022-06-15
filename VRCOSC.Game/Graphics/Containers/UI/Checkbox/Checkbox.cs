// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Containers.UI.Button;

namespace VRCOSC.Game.Graphics.Containers.UI.Checkbox;

public class Checkbox : IconButton
{
    public BindableBool State { get; init; } = new();
    public IconUsage IconOn { get; init; } = FontAwesome.Solid.Check;
    public IconUsage IconOff { get; init; } = FontAwesome.Solid.Get(0xf00d);
    public Colour4 ColourOn { get; init; } = VRCOSCColour.Green;
    public Colour4 ColourOff { get; init; } = VRCOSCColour.Red;

    [BackgroundDependencyLoader]
    private void load()
    {
        State.BindValueChanged(e =>
        {
            BackgroundColour.Value = e.NewValue ? ColourOn : ColourOff;
            Icon.Value = e.NewValue ? IconOn : IconOff;
        }, true);

        Action = State.Toggle;
    }
}
