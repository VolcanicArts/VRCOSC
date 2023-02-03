// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net;
using System.Threading.Tasks;

namespace VRCOSC.Game.OSC.Client;

public abstract class OscClient
{
    private readonly OscSender sender = new();
    private readonly OscReceiver receiver = new();

    protected Action<byte[]>? OnRawDataReceived;

    public void Initialise(string ipAddress, int sendPort, int receivePort)
    {
        sender.Initialise(new IPEndPoint(IPAddress.Parse(ipAddress), sendPort));
        receiver.Initialise(new IPEndPoint(IPAddress.Parse(ipAddress), receivePort));

        receiver.OnRawDataReceived += byteData => OnRawDataReceived?.Invoke(byteData);
    }

    public void Enable()
    {
        sender.Enable();
        receiver.Enable();
    }

    public void DisableSender()
    {
        sender.Disable();
    }

    public async Task DisableReceiver()
    {
        await receiver.Disable();
    }

    public void SendByteData(byte[] data)
    {
        sender.Send(data);
    }
}
