// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Extensions.EnumExtensions;

namespace VRCOSC.Game.OSC.Client;

public abstract class OscClient
{
    protected readonly OscSender Sender = new();
    protected readonly OscReceiver Receiver = new();

    public void Initialise(IPEndPoint sendEndpoint, IPEndPoint receiveEndpoint)
    {
        Sender.Initialise(sendEndpoint);
        Receiver.Initialise(receiveEndpoint);
    }

    public void Enable(OscClientFlag flag)
    {
        if (flag.HasFlagFast(OscClientFlag.Send)) Sender.Enable();
        if (flag.HasFlagFast(OscClientFlag.Receive)) Receiver.Enable();
    }

    public async Task Disable(OscClientFlag flag)
    {
        if (flag.HasFlagFast(OscClientFlag.Send)) Sender.Disable();
        if (flag.HasFlagFast(OscClientFlag.Receive)) await Receiver.Disable();
    }

    public void SendByteData(byte[] data)
    {
        Sender.Send(data);
    }
}

[Flags]
public enum OscClientFlag
{
    Send = 1 << 0,
    Receive = 1 << 1
}
