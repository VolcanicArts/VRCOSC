// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public class VRCOSCGameBase : osu.Framework.Game
{
    protected VRCOSCGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new Vector2(1366, 768)
        });
    }

    protected override Container<Drawable> Content { get; }

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        host.Window.Title = "VRCOSC";
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));
    }
}
