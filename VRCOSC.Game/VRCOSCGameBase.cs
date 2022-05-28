// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Modules;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public class VRCOSCGameBase : osu.Framework.Game
{
    private DependencyContainer dependencyContainer;

    protected VRCOSCGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = new Vector2(1366, 768)
        });
    }

    protected override Container<Drawable> Content { get; }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
    {
        return dependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));
    }

    [BackgroundDependencyLoader]
    private void load(Storage storage, GameHost host)
    {
        host.Window.Title = "VRCOSC";
        dependencyContainer.CacheAs(new ModuleManager(storage));
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));
    }
}
