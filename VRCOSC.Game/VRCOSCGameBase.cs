// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osuTK;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public partial class VRCOSCGameBase : osu.Framework.Game
{
    private static Version assemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

    protected string Version => $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

    protected DependencyContainer DependencyContainer = null!;

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

    protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults() => new Dictionary<FrameworkSetting, object>
    {
        { FrameworkSetting.FrameSync, FrameSync.VSync }
    };

    protected override Container<Drawable> Content { get; }

    protected VRCOSCGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer
        {
            TargetDrawSize = Vector2.One
        });
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));
    }
}
