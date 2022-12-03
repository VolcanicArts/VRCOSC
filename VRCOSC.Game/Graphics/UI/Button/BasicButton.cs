// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace VRCOSC.Game.Graphics.UI.Button;

public partial class BasicButton : VRCOSCButton
{
    private Colour4 backgroundColourStateOff = VRCOSCColour.Red;
    private Colour4 backgroundColourStateOn = VRCOSCColour.Green;

    protected Drawable BackgroundBox = null!;

    public BindableBool State = new();
    public bool Stateful { get; init; }

    public Colour4 BackgroundColour
    {
        get => backgroundColourStateOff;
        set
        {
            backgroundColourStateOn = value;
            backgroundColourStateOff = value;
            updateBackgroundColour();
        }
    }

    public Colour4 BackgroundColourStateOn
    {
        get => backgroundColourStateOn;
        set
        {
            backgroundColourStateOn = value;
            updateBackgroundColour();
        }
    }

    public Colour4 BackgroundColourStateOff
    {
        get => backgroundColourStateOff;
        set
        {
            backgroundColourStateOff = value;
            updateBackgroundColour();
        }
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = BackgroundBox = CreateBackground();

        State.BindValueChanged(_ => updateBackgroundColour(), true);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        updateBackgroundColour();
    }

    private void updateBackgroundColour()
    {
        if (!IsLoaded) return;

        if (Stateful)
            BackgroundBox.Colour = State.Value ? backgroundColourStateOn : backgroundColourStateOff;
        else
            BackgroundBox.Colour = backgroundColourStateOff;
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (Enabled.Value) State.Toggle();
        return base.OnClick(e);
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
