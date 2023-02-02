// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;

namespace VRCOSC.Game.OSC.Client;

public class OscSender
{
    private Socket? socket;
    private IPEndPoint endPoint = null!;

    public void Initialise(IPEndPoint endPoint)
    {
        this.endPoint = endPoint;
    }

    public void Enable()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(endPoint);
    }

    public void Disable()
    {
        socket = null;
    }

    public void Send(byte[] data)
    {
        socket?.Send(data);
    }
}
