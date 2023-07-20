// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Extensions.EnumExtensions;

namespace VRCOSC.Game.OSC.Client;

public abstract class OscClient
{
    private readonly OscSender sender = new();
    private readonly OscReceiver receiver = new();

    public Action<OscMessage>? OnMessageSent;
    public Action<OscMessage>? OnMessageReceived;

    public Action<byte[]>? OnDataSent;
    public Action<byte[]>? OnDataReceived;

    public OscClient()
    {
        receiver.OnRawDataReceived += data =>
        {
            OnDataReceived?.Invoke(data);

            var message = OscDecoder.Decode(data);
            if (message is null) return;

            OnMessageReceived?.Invoke(message);
        };
    }

    public void Initialise(IPEndPoint sendEndpoint, IPEndPoint receiveEndpoint)
    {
        sender.Initialise(sendEndpoint);
        receiver.Initialise(receiveEndpoint);
    }

    public void Enable(OscClientFlag flag)
    {
        if (flag.HasFlagFast(OscClientFlag.Send)) sender.Enable();
        if (flag.HasFlagFast(OscClientFlag.Receive)) receiver.Enable();
    }

    public async Task Disable(OscClientFlag flag)
    {
        if (flag.HasFlagFast(OscClientFlag.Send)) sender.Disable();
        if (flag.HasFlagFast(OscClientFlag.Receive)) await receiver.Disable();
    }

    public void SendValue(string address, object value) => SendValues(address, new List<object> { value });
    public void SendValues(string address, List<object> values) => SendMessage(new OscMessage(address, values));

    public void SendMessage(OscMessage message)
    {
        Send(OscEncoder.Encode(message));
        OnMessageSent?.Invoke(message);
    }

    public void Send(byte[] data)
    {
        sender.Send(data);
        OnDataSent?.Invoke(data);
    }
}

[Flags]
public enum OscClientFlag
{
    Send = 1 << 0,
    Receive = 1 << 1
}
