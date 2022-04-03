// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace VRCOSC.Game.Graphics.Containers;

public class VRCOSCScrollContainer : VRCOSCScrollContainer<Drawable>
{
}

public class VRCOSCScrollContainer<T> : BasicScrollContainer where T : Drawable
{
    private readonly FillFlowContainer<T> scrollContentFlow;

    public new IReadOnlyList<T> ScrollContent
    {
        get => scrollContentFlow.Children;
        set => scrollContentFlow.Children = value;
    }

    public VRCOSCScrollContainer()
    {
        ClampExtension = 20;
        ScrollbarVisible = false;

        Child = scrollContentFlow = new FillFlowContainer<T>
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
        scrollContentFlow.Add(drawable);
    }
}
