// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using VRCOSC.Game.Graphics.Containers.Screens;

namespace VRCOSC.Game;

public class VRCOSCGame : VRCOSCGameBase
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new ScreenManager();
    }
}
