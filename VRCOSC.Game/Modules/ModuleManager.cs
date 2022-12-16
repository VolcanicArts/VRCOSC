// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Modules.Modules.ChatBoxText;
using VRCOSC.Game.Modules.Modules.Clock;
using VRCOSC.Game.Modules.Modules.Discord;
using VRCOSC.Game.Modules.Modules.HardwareStats;
using VRCOSC.Game.Modules.Modules.Heartrate.HypeRate;
using VRCOSC.Game.Modules.Modules.Heartrate.Pulsoid;
using VRCOSC.Game.Modules.Modules.Media;
using VRCOSC.Game.Modules.Modules.OpenVR;
using VRCOSC.Game.Modules.Modules.Random;

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
        typeof(RandomBoolModule),
        typeof(RandomIntModule),
        typeof(RandomFloatModule)
    };

    private readonly TerminalLogger terminal = new(nameof(ModuleManager));

    [BackgroundDependencyLoader]
    private void load()
    {
        module_types.ForEach(type =>
        {
            var module = (Module)Activator.CreateInstance(type)!;
            LoadComponent(module);
            AddInternal(module);
        });
    }

    public async Task Start(CancellationToken startToken)
    {
        if (this.All(module => !module.Enabled.Value))
        {
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");
            return;
        }

        foreach (var module in this)
        {
            await module.start(startToken);
        }
    }

    public async Task Stop()
    {
        foreach (var module in this)
        {
            await module.stop();
        }
    }

    public IEnumerator<Module> GetEnumerator() => InternalChildren.Select(child => (Module)child).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
