// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace VRCOSC.Game.Graphics.UI.Button;

public class TextButton : BasicButton
{
    private string text = string.Empty;

    private SpriteText? spriteText;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            if (spriteText != null) spriteText.Text = text;
        }
    }

    public float FontSize { get; init; } = 30;

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(spriteText = new SpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Font = FrameworkFont.Regular.With(size: FontSize),
            Text = Text,
            Shadow = true
        });
    }
}
