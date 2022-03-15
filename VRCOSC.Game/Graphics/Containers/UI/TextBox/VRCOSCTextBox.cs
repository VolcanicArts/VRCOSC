// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.Containers.UI.TextBox;

public class VRCOSCTextBox : BasicTextBox
{
    public VRCOSCTextBox()
    {
        BackgroundFocused = VRCOSCColour.Gray6;
        BackgroundUnfocused = VRCOSCColour.Gray4;
        Masking = true;
        CornerRadius = 10;
    }

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        host.Window.Resized += () => Scheduler.AddOnce(KillFocus);
    }

    protected override SpriteText CreatePlaceholder()
    {
        var fadingPlaceholderText = base.CreatePlaceholder();
        fadingPlaceholderText.Colour = Color4.White.Opacity(0.5f);
        return fadingPlaceholderText;
    }
}
