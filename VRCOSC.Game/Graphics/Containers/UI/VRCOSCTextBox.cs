// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Platform;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class VRCOSCTextBox : BasicTextBox
{
    public Action<string>? CharWritten { get; init; }

    public VRCOSCTextBox()
    {
        BackgroundFocused = VRCOSCColour.Gray6;
        BackgroundUnfocused = VRCOSCColour.Gray4;
        Masking = true;
        CornerRadius = 10;
        CommitOnFocusLost = true;
    }

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        host.Window.Resized += () => Scheduler.AddOnce(KillFocus);
    }

    public override bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
    {
        var result = base.OnPressed(e);
        CharWritten?.Invoke(Text);
        return result;
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        var result = base.OnKeyDown(e);
        CharWritten?.Invoke(Text);
        return result;
    }

    protected override SpriteText CreatePlaceholder()
    {
        return base.CreatePlaceholder().With(t => t.Colour = Colour4.White.Opacity(0.5f));
    }
}
