// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Threading.Tasks;

namespace VRCOSC.Game.OSC.Client;

public abstract class OscClient
{
    protected readonly OscSender Sender = new();
    protected readonly OscReceiver Receiver = new();

    public void Initialise(string ipAddress, int sendPort, int receivePort)
    {
        Sender.Initialise(new IPEndPoint(IPAddress.Parse(ipAddress), sendPort));
        Receiver.Initialise(new IPEndPoint(IPAddress.Parse(ipAddress), receivePort));
    }

    public void Enable()
    {
        Sender.Enable();
        Receiver.Enable();
    }

    public void DisableSender()
    {
        Sender.Disable();
    }

    public async Task DisableReceiver()
    {
        await Receiver.Disable();
    }

    public void SendByteData(byte[] data)
    {
        Sender.Send(data);
    }
}
