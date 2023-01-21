// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using VRCOSC.Game.Modules.Modules.ChatBoxText;
using VRCOSC.Game.Modules.Modules.Clock;
using VRCOSC.Game.Modules.Modules.Discord;
using VRCOSC.Game.Modules.Modules.HardwareStats;
using VRCOSC.Game.Modules.Modules.Heartrate.HypeRate;
using VRCOSC.Game.Modules.Modules.Heartrate.Pulsoid;
using VRCOSC.Game.Modules.Modules.Media;
using VRCOSC.Game.Modules.Modules.OpenVR;
using VRCOSC.Game.Modules.Modules.Random;
using VRCOSC.Game.Modules.Modules.Weather;

namespace VRCOSC.Game.Modules;

public sealed partial class ModuleManager : CompositeComponent, IEnumerable<Module>
{
    private static readonly IReadOnlyList<Type> module_types = new[]
    {
        typeof(HypeRateModule),
        typeof(PulsoidModule),
        typeof(OpenVRStatisticsModule),
        typeof(OpenVRControllerStatisticsModule),
        typeof(GestureExtensionsModule),
        typeof(MediaModule),
        typeof(DiscordModule),
        typeof(ClockModule),
        typeof(ChatBoxTextModule),
        typeof(HardwareStatsModule),
        typeof(WeatherModule),
        typeof(RandomBoolModule),
        typeof(RandomIntModule),
        typeof(RandomFloatModule)
    };

    private readonly TerminalLogger terminal = new(nameof(ModuleManager));

    [Resolved]
    private Storage storage { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        module_types.ForEach(type =>
        {
            var module = (Module)Activator.CreateInstance(type)!;
            LoadComponent(module);
            AddInternal(module);
        });

        loadExternalModules();
    }

    private void loadExternalModules()
    {
        var moduleDirectoryPath = storage.GetStorageForDirectory("custom").GetFullPath(string.Empty, true);
        Directory.GetFiles(moduleDirectoryPath, "*.dll").ForEach(createModule);
    }

    private void createModule(string dllPath)
    {
        var moduleAssemblyTypes = Assembly.LoadFile(dllPath).GetTypes();
        var moduleType = moduleAssemblyTypes.First(type => type.IsSubclassOf(typeof(Module)) && !type.IsAbstract);
        var moduleInstance = (Module)Activator.CreateInstance(moduleType)!;
        LoadComponent(moduleInstance);
        AddInternal(moduleInstance);
    }

    public void Start()
    {
        if (this.All(module => !module.Enabled.Value))
        {
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");
            return;
        }

        foreach (var module in this)
        {
            module.start();
        }
    }

    public void Stop()
    {
        foreach (var module in this)
        {
            module.stop();
        }
    }

    public IEnumerator<Module> GetEnumerator() => InternalChildren.Select(child => (Module)child).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
