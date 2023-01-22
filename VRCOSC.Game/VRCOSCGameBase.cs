// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Resources;

namespace VRCOSC.Game;

public partial class VRCOSCGameBase : osu.Framework.Game
{
#if DEBUG
    private const string base_game_name = @"VRCOSC-Development";
#else
    private const string base_game_name = @"VRCOSC";
#endif

    private static readonly Size default_size = new(1450, 768);

    protected DependencyContainer DependencyContainer = null!;
    protected VRCOSCConfigManager ConfigManager = null!;

    private Bindable<string> versionBindable = null!;

    private static Version assemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

    protected string Version => $@"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

    protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults()
        => new Dictionary<FrameworkSetting, object>
        {
            { FrameworkSetting.WindowedSize, default_size }
        };

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

    [BackgroundDependencyLoader]
    private void load(GameHost host, Storage storage)
    {
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));

        DependencyContainer.CacheAs(ConfigManager = new VRCOSCConfigManager(storage));

        versionBindable = ConfigManager.GetBindable<string>(VRCOSCSetting.Version);
        versionBindable.BindValueChanged(version => host.Window.Title = $"{base_game_name} {version.NewValue}", true);
    }
}
