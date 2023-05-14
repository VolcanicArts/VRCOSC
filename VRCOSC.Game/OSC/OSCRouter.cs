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

    public readonly List<OscSender> Senders = new();
    public readonly List<OscReceiver> Receivers = new();

    public OSCRouter(VRChatOscClient vrChatOscClient)
    {
        this.vrChatOscClient = vrChatOscClient;
    }

    public void Initialise(IEnumerable<RouterData> data)
    {
        data.ForEach(routerData =>
        {
            var pair = routerData.Endpoints;

            if (!string.IsNullOrEmpty(pair.SendAddress.Value))
            {
                var sender = new OscSender();
                sender.Initialise(new IPEndPoint(IPAddress.Parse(pair.SendAddress.Value), pair.SendPort.Value));
                Senders.Add(sender);

                terminal.Log($"Initialising sender labelled {routerData.Label} on {pair.SendAddress}:{pair.SendPort}");
            }

            if (!string.IsNullOrEmpty(pair.ReceiveAddress.Value))
            {
                var receiver = new OscReceiver();
                receiver.Initialise(new IPEndPoint(IPAddress.Parse(pair.ReceiveAddress.Value), pair.ReceivePort.Value));
                Receivers.Add(receiver);

                terminal.Log($"Initialising receiver labelled {routerData.Label} on {pair.ReceiveAddress}:{pair.ReceivePort}");
            }
        });
    }

    // Anything coming from VRC has to be parsed first, hence listening for parameters and not the raw data
    // I have no idea why, it should just work forwarding the raw bytes
    public void Enable()
    {
        Senders.ForEach(sender =>
        {
            vrChatOscClient.OnParameterReceived += parameter => sender.Send(parameter.Encode());
            sender.Enable();
        });

        Receivers.ForEach(receiver =>
        {
            receiver.OnRawDataReceived += vrChatOscClient.SendByteData;
            receiver.Enable();
        });
    }

    public async Task Disable()
    {
        foreach (var sender in Senders) sender.Disable();
        foreach (var receiver in Receivers) await receiver.Disable();

        Senders.Clear();
        Receivers.Clear();
    }
}

public class OSCRouterEndpoints
{
    public readonly Bindable<string> SendAddress = new(string.Empty);
    public readonly Bindable<int> SendPort = new();
    public readonly Bindable<string> ReceiveAddress = new(string.Empty);
    public readonly Bindable<int> ReceivePort = new();
}
