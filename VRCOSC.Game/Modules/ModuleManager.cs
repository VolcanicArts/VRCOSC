// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CoreOSC.IO;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public sealed class ModuleManager : Container<ModuleGroup>
{
    private const string osc_ip_address = "127.0.0.1";
    private const int osc_send_port = 9000;
    private const int osc_receive_port = 9001;

    private readonly UdpClient sendingClient = new(osc_ip_address, osc_send_port);

    public BindableBool Running = new();

    [BackgroundDependencyLoader]
    private void load(Storage storage)
    {
        List<Module> modules = ReflectiveEnumerator.GetEnumerableOfType<Module>();

        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            var moduleGroup = new ModuleGroup(type);

            foreach (var module in modules.Where(module => module.Type.Equals(type)))
            {
                module.DataManager = new ModuleDataManager(storage, module.GetType().Name);
                module.OscClient = sendingClient;
                module.CreateAttributes();
                module.DataManager.LoadData();
                moduleGroup.Add(new ModuleContainer(module));
            }

            Add(moduleGroup);
        }
    }

    public void Start()
    {
        this.ForEach(child => child.Start());
        Running.Value = true;

        Task.Factory.StartNew(beginListening, TaskCreationOptions.LongRunning);
    }

    public void Stop()
    {
        Running.Value = false;
        this.ForEach(child => child.Stop());
    }

    private async void beginListening()
    {
        IPEndPoint receiveEndpoint = new IPEndPoint(IPAddress.Parse(osc_ip_address), osc_receive_port);
        var receivingClient = new UdpClient(receiveEndpoint);

        while (true)
        {
            if (!Running.Value) break;

            var message = await receivingClient.ReceiveMessageAsync();
            this.ForEach(child => child.OnOSCMessage(message));
        }

        receivingClient.Dispose();
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        sendingClient.Dispose();
    }
}
