// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class TextButton : VRCOSCButton
{
    protected internal string Text { get; init; } = string.Empty;
    protected internal float FontSize { get; init; } = 30;

    [BackgroundDependencyLoader]
    private void load()
    {
        AddInternal(new SpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Font = FrameworkFont.Regular.With(size: FontSize),
            Text = Text,
            Shadow = true
        });
    }
}
