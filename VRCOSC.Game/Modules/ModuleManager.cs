// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreOSC.IO;
using osu.Framework.Allocation;
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

    private readonly UdpClient sendingClient;
    private readonly UdpClient receivingClient;
    private CancellationTokenSource? tokenSource;

    public Action<string, object> OnParameterSent;
    public Action<string, object> OnParameterReceived;

    public ModuleManager()
    {
        sendingClient = new UdpClient(osc_ip_address, osc_send_port);
        var receiveEndpoint = new IPEndPoint(IPAddress.Parse(osc_ip_address), osc_receive_port);
        receivingClient = new UdpClient(receiveEndpoint);
    }

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
                module.OnParameterSent += (key, value) => OnParameterSent.Invoke(key, value);
                module.OnParameterReceived += (key, value) => OnParameterReceived.Invoke(key, value);
                moduleGroup.Add(new ModuleContainer(module));
            }

            Add(moduleGroup);
        }
    }

    public void Start()
    {
        tokenSource = new CancellationTokenSource();
        this.ForEach(child => child.Start());
        Task.Factory.StartNew(beginListening);
    }

    public void Stop()
    {
        tokenSource?.Cancel();
        this.ForEach(child => child.Stop());
    }

    private async void beginListening()
    {
        if (tokenSource == null) throw new AggregateException("Cancellation token is null when trying to listen for OSC messages");

        while (!tokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var message = await receivingClient.ReceiveMessageAsync();
                this.ForEach(child => child.OnOSCMessage(message));
            }
            catch (SocketException _) { }
        }
    }
}
