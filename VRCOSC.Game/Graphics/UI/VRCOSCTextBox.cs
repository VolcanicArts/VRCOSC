// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;

namespace VRCOSC.Game.Graphics.UI;

public class VRCOSCTextBox : BasicTextBox
{
    public VRCOSCTextBox()
    {
        BackgroundFocused = VRCOSCColour.Gray3;
        BackgroundUnfocused = VRCOSCColour.Gray3;
        Masking = true;
        CommitOnFocusLost = true;
    }

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        host.Window.Resized += () => Scheduler.AddOnce(KillFocus);
    }

    protected override SpriteText CreatePlaceholder()
    {
        return base.CreatePlaceholder().With(t => t.Colour = Colour4.White.Opacity(0.5f));
    }
}
