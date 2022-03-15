// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace VRCOSC.Game.Graphics.Containers.UI;

public class TextButton : VRCOSCButton
{
    protected internal string Text { get; init; } = string.Empty;
    protected internal float FontSize { get; init; } = 30;

    [BackgroundDependencyLoader]
    private void load()
    {
        AddInternal(new TextFlowContainer(t =>
        {
            t.Font = FrameworkFont.Regular.With(size: FontSize);
            t.Shadow = true;
            t.ShadowColour = Color4.Black.Opacity(0.5f);
            t.ShadowOffset = new Vector2(0.0f, 0.025f);
        })
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            TextAnchor = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Text = Text
        });
    }
}
