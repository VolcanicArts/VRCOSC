// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace VRCOSC.Game.Graphics.Containers;

public class VRCOSCScrollContainer<T> : BasicScrollContainer where T : Drawable
{
    public new FillFlowContainer<T> Content { get; private set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        ClampExtension = 20;
        ScrollbarVisible = false;
        Child = Content = new FillFlowContainer<T>
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(10),
            Padding = new MarginPadding(10)
        };
    }

    public void Add(T drawable)
    {
        Content.Add(drawable);
    }
}
