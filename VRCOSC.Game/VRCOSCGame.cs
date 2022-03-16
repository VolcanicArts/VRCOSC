// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.Screens;

namespace VRCOSC.Game;

public class VRCOSCGame : VRCOSCGameBase
{
    [Cached]
    private ScreenManager screenManager = new()
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        RelativeSizeAxes = Axes.Both
    };

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = screenManager;
    }
}
