// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;

namespace VRCOSC.Game;

[Cached]
public abstract partial class VRCOSCGame : VRCOSCGameBase
{
    protected VRCOSCGame()
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
    }
}
