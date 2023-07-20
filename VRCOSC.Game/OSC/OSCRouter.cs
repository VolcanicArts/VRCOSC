// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules;
using VRCOSC.Game.OSC.Client;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.OSC;

public class OSCRouter
{
    private readonly VRChatOscClient vrChatOscClient;
    private readonly TerminalLogger terminal = new("OSCRouter");

    private readonly List<OscSender> senders = new();
    private readonly List<OscReceiver> receivers = new();

    public OSCRouter(VRChatOscClient vrChatOscClient)
    {
        this.vrChatOscClient = vrChatOscClient;
    }

    public void Start(IEnumerable<RouterData> data)
    {
        data.ForEach(routerData =>
        {
            var pair = routerData.Endpoints;

            if (!string.IsNullOrEmpty(pair.SendAddress.Value))
            {
                var sender = new OscSender();
                sender.Initialise(new IPEndPoint(IPAddress.Parse(pair.SendAddress.Value), pair.SendPort.Value));
                senders.Add(sender);

                terminal.Log($"Initialising sender labelled {routerData.Label} on {pair.SendAddress}:{pair.SendPort}");
            }

            if (!string.IsNullOrEmpty(pair.ReceiveAddress.Value))
            {
                var receiver = new OscReceiver();
                receiver.Initialise(new IPEndPoint(IPAddress.Parse(pair.ReceiveAddress.Value), pair.ReceivePort.Value));
                receivers.Add(receiver);

                terminal.Log($"Initialising receiver labelled {routerData.Label} on {pair.ReceiveAddress}:{pair.ReceivePort}");
            }
        });

        senders.ForEach(sender =>
        {
            vrChatOscClient.OnDataReceived += sender.Send;
            sender.Enable();
        });

        receivers.ForEach(receiver =>
        {
            receiver.OnRawDataReceived += vrChatOscClient.Send;
            receiver.Enable();
        });
    }

    public async Task Disable()
    {
        foreach (var sender in senders) sender.Disable();
        foreach (var receiver in receivers) await receiver.Disable();

        senders.Clear();
        receivers.Clear();
    }
}

public class OSCRouterEndpoints
{
    public readonly Bindable<string> SendAddress = new(string.Empty);
    public readonly Bindable<int> SendPort = new();
    public readonly Bindable<string> ReceiveAddress = new(string.Empty);
    public readonly Bindable<int> ReceivePort = new();
}
