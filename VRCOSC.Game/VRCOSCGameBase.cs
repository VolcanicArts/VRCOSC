// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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
    [Resolved]
    private GameHost host { get; set; } = null!;

    protected DependencyContainer DependencyContainer = null!;
    protected VRCOSCConfigManager ConfigManager = null!;

    private Bindable<string> versionBindable = null!;

    private static Version assemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

    protected string Version => $@"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        => DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

    protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults() => new Dictionary<FrameworkSetting, object>
    {
        { FrameworkSetting.FrameSync, FrameSync.VSync }
    };

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        Resources.AddStore(new DllResourceStore(typeof(VRCOSCResources).Assembly));

        DependencyContainer.CacheAs(ConfigManager = new VRCOSCConfigManager(storage));

        versionBindable = ConfigManager.GetBindable<string>(VRCOSCSetting.Version);
        versionBindable.BindValueChanged(version => host.Window.Title = $"{host.Name} {version.NewValue}", true);

        Window.WindowState = ConfigManager.Get<WindowState>(VRCOSCSetting.WindowState);
    }

    protected override bool OnExiting()
    {
        ConfigManager.SetValue(VRCOSCSetting.WindowState, Window.WindowState);
        return false;
    }
}
