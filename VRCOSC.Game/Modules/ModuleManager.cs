// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CoreOSC.IO;
using Markdig.Helpers;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public class ModuleManager
{
    private const string osc_ip_address = "127.0.0.1";
    private const int osc_receive_port = 9001;

    public Dictionary<ModuleType, OrderedList<Module>> Modules { get; } = new();

    private readonly BindableBool Running = new();

    public ModuleManager(Storage storage)
    {
        IEnumerable<Module> modules = ReflectiveEnumerator.GetEnumerableOfType<Module>(storage);
        modules.ForEach(module =>
        {
            module.DataManager.LoadData();
            addModule(module);
        });
        sortModules();
    }

    private void addModule(Module? module)
    {
        if (module == null) return;

        var list = Modules.GetValueOrDefault(module.Type, new OrderedList<Module>());
        list.Add(module);
        Modules.TryAdd(module.Type, list);
    }

    private void sortModules()
    {
        Dictionary<ModuleType, OrderedList<Module>> localList = new(Modules);
        Modules.Clear();

        foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
        {
            if (!localList.TryGetValue(moduleType, out var moduleList)) return;

            Modules.Add(moduleType, moduleList);
        }
    }

    public void Start(Scheduler scheduler)
    {
        Running.Value = true;
        Modules.Values.ForEach(modules =>
        {
            modules.ForEach(module =>
            {
                if (!module.DataManager.Enabled) return;

                module.Start();

                if (double.IsPositiveInfinity(module.DeltaUpdate)) return;

                scheduler.Add(module.Update);
                scheduler.AddDelayed(module.Update, module.DeltaUpdate, true);
            });
        });

        Task.Factory.StartNew(beginListening, TaskCreationOptions.LongRunning);
    }

    public void Stop(Scheduler scheduler)
    {
        Running.Value = false;
        scheduler.CancelDelayedTasks();
        Modules.Values.ForEach(modules =>
        {
            modules.ForEach(module =>
            {
                if (module.DataManager.Enabled) module.Stop();
            });
        });
    }

    private async void beginListening()
    {
        IPEndPoint oscReceiveEndpoint = new IPEndPoint(IPAddress.Parse(osc_ip_address), osc_receive_port);
        var oscReceivingClient = new UdpClient(oscReceiveEndpoint);

        while (true)
        {
            if (!Running.Value) break;

            var message = await oscReceivingClient.ReceiveMessageAsync();
            Modules.Values.ForEach(modules =>
            {
                modules.ForEach(module =>
                {
                    if (module.DataManager.Enabled) module.OnOSCMessage(message);
                });
            });
        }

        oscReceivingClient.Dispose();
    }
}
