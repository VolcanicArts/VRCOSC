﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Tabs;

public partial class TabBar : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Box
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Colour = Colours.Mid
        });
    }
}