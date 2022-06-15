// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Containers.Screens;
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
    private bool running;
    private bool autoStarted;

    public Action<string, object> OnParameterSent;
    public Action<string, object> OnParameterReceived;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; }

    [Resolved]
    private ScreenManager screenManager { get; set; }

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

        var moduleStorage = storage.GetStorageForDirectory("modules");

        foreach (ModuleType type in Enum.GetValues(typeof(ModuleType)))
        {
            var moduleGroup = new ModuleGroup(type);

            foreach (var module in modules.Where(module => module.Type.Equals(type)))
            {
                module.Initialise(moduleStorage, sendingClient);
                module.CreateAttributes();
                module.PerformLoad();
                module.OnParameterSent += (key, value) => OnParameterSent.Invoke(key, value);
                module.OnParameterReceived += (key, value) => OnParameterReceived.Invoke(key, value);
                moduleGroup.Add(new ModuleContainer(module));
            }

            Add(moduleGroup);
        }

        Task.Factory.StartNew(checkForVrChat);
    }

    private void checkForVrChat()
    {
        while (true)
        {
            var vrChat = Process.GetProcessesByName("vrchat");

            var autoStartStop = configManager.Get<bool>(VRCOSCSetting.AutoStartStop);

            if (vrChat.Length != 0 && autoStartStop && !running && !autoStarted)
            {
                screenManager.ShowTerminal();
                autoStarted = true;
            }

            if (vrChat.Length == 0 && autoStartStop && running)
            {
                screenManager.HideTerminal();
                autoStarted = false;
            }

            Task.Delay(10000);
        }
    }

    public void Start()
    {
        tokenSource = new CancellationTokenSource();
        this.ForEach(child => child.Start());
        Task.Factory.StartNew(beginListening);
        running = true;
    }

    public void Stop()
    {
        running = false;
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
