// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.UI.Button;

public partial class BasicButton : VRCOSCButton
{
    private Drawable backgroundBox = null!;

    public Bindable<bool> State = new();
    public bool Stateful { get; set; }

    public Colour4 BackgroundColour
    {
        get => BackgroundColourStateOff;
        set
        {
            BackgroundColourStateOn = value;
            BackgroundColourStateOff = value;
        }
    }

    public Colour4 BackgroundColourStateOn { get; set; } = ThemeManager.Current[ThemeAttribute.Success];
    public Colour4 BackgroundColourStateOff { get; set; } = ThemeManager.Current[ThemeAttribute.Failure];

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(backgroundBox = new Box
        {
            RelativeSizeAxes = Axes.Both
        });
    }

    protected override void Update()
    {
        base.Update();

        if (Stateful)
            backgroundBox.Colour = State.Value ? BackgroundColourStateOn : BackgroundColourStateOff;
        else
            backgroundBox.Colour = BackgroundColourStateOff;
    }

    protected override bool OnClick(ClickEvent e)
    {
        if (Enabled.Value) State.Value = !State.Value;
        return base.OnClick(e);
    }
}
