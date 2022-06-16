// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace VRCOSC.Game.Graphics.Containers.UI.Dynamic;

public class StatefulButton : VRCOSCButton
{
    public BindableBool State = new();

    public Colour4 BackgroundColourStateTrue { get; init; } = VRCOSCColour.Green;
    public Colour4 BackgroundColourStateFalse { get; init; } = VRCOSCColour.Red;

    [BackgroundDependencyLoader]
    private void load()
    {
        Drawable backgroundBox;

        InternalChild = backgroundBox = CreateBackground();

        State.BindValueChanged(_ => backgroundBox.Colour = State.Value ? BackgroundColourStateTrue : BackgroundColourStateFalse, true);
        Action += State.Toggle;
    }

    public virtual Drawable CreateBackground()
    {
        return new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        };
    }
}
