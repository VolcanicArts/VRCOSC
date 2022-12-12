// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions.IEnumerableExtensions;
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

namespace VRCOSC.Game.Modules;

public sealed partial class ModuleManager : IEnumerable<Module>
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

    public readonly List<Module> Modules = new();

    public void Initialise(GameHost host, Storage storage, GameManager gameManager)
    {
        var moduleStorage = storage.GetStorageForDirectory("modules");
        module_types.ForEach(type =>
        {
            var module = (Module)Activator.CreateInstance(type)!;
            module.Initialise(host, moduleStorage, gameManager);
            Modules.Add(module);
        });
    }

    public async Task Start(CancellationToken startToken)
    {
        if (Modules.All(module => !module.Enabled.Value))
        {
            terminal.Log("You have no modules selected!\nSelect some modules to begin using VRCOSC");
            return;
        }

        foreach (var module in Modules)
        {
            await module.start(startToken);
        }
    }

    public async Task Stop()
    {
        foreach (var module in Modules)
        {
            await module.stop();
        }
    }

    public IEnumerator<Module> GetEnumerator() => Modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
