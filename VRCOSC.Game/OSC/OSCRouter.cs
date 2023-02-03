// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Logging;
using VRCOSC.Game.OSC.Client;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.OSC;

public class OSCRouter
{
    private readonly VRChatOscClient vrChatOscClient;

    public readonly List<OscSender> Senders = new();
    public readonly List<OscReceiver> Receivers = new();

    public OSCRouter(VRChatOscClient vrChatOscClient)
    {
        this.vrChatOscClient = vrChatOscClient;
    }

    public void Initialise(List<OSCRouterEndpoints> pairs)
    {
        pairs.ForEach(pair =>
        {
            var sender = new OscSender();
            var receiver = new OscReceiver();

            sender.Initialise(pair.SendEndpoint);
            receiver.Initialise(pair.ReceiveEndPoint);

            Logger.Log($"Initialising new router on {pair.ReceiveEndPoint}:{pair.SendEndpoint}");

            Senders.Add(sender);
            Receivers.Add(receiver);
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
    public required IPEndPoint SendEndpoint { get; init; }
    public required IPEndPoint ReceiveEndPoint { get; init; }
}
