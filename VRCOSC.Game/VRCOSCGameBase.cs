// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public class VRCOSCGameBase : osu.Framework.Game
{
    private static readonly Vector2 default_size_v = new(1450, 768);
    private static readonly Size default_size = new(1450, 768);

    protected DependencyContainer DependencyContainer = null!;

    protected VRCOSCGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = default_size_v
        });
    }

    protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults()
    {
        return new Dictionary<FrameworkSetting, object>
        {
            { FrameworkSetting.WindowedSize, default_size }
        };
    }

    protected override Container<Drawable> Content { get; }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
    {
        return DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));
    }

    [BackgroundDependencyLoader]
    private void load(GameHost host, Storage storage)
    {
        host.Window.Title = "VRCOSC";
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));

        DependencyContainer.CacheAs(new VRCOSCConfigManager(storage));
    }
}
