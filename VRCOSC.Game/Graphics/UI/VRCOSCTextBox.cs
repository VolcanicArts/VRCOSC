// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;

namespace VRCOSC.Game.Graphics.UI;

public sealed class VRCOSCTextBox : BasicTextBox
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    public VRCOSCTextBox()
    {
        BackgroundFocused = VRCOSCColour.Gray3;
        BackgroundUnfocused = VRCOSCColour.Gray3;
        Masking = true;
        CommitOnFocusLost = true;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        host.Window.Resized += KillFocus;
    }

    protected override void KillFocus() => Schedule(() => base.KillFocus());

    protected override SpriteText CreatePlaceholder()
    {
        return base.CreatePlaceholder().With(t => t.Colour = Colour4.White.Opacity(0.5f));
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        host.Window.Resized -= KillFocus;
    }
}
