// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
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

    public Vector2 ContentSpacing { get; init; } = new(10);
    public MarginPadding ContentPadding { get; init; } = new(10);

    public new IReadOnlyList<T> ScrollContent
    {
        get => scrollContentFlow.Children;
        init => scrollContentFlow.Children = value;
    }

    public VRCOSCScrollContainer()
    {
        ClampExtension = 20;
        ScrollbarVisible = false;

        Child = scrollContentFlow = new FillFlowContainer<T>
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        scrollContentFlow.Spacing = ContentSpacing;
        scrollContentFlow.Padding = ContentPadding;
    }

    public void Add(T drawable)
    {
        scrollContentFlow.Add(drawable);
    }

    public override void Clear(bool disposeChildren)
    {
        scrollContentFlow.Clear(disposeChildren);
    }
}
