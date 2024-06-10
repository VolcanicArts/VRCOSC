// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OSC.Client;

public class OscSender
{
    private Socket? socket;
    public IPEndPoint? EndPoint { get; private set; }

    public void Initialise(IPEndPoint endPoint)
    {
        EndPoint = endPoint;
    }

    public bool Enable()
    {
        Debug.Assert(EndPoint is not null);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            socket.Connect(EndPoint);
            return true;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(OscSender)} experienced an exception");
            return false;
        }
    }

    public void Disable()
    {
        socket = null;
    }

    public void Send(byte[] data)
    {
        if (socket?.Connected ?? false)
            socket?.Send(data);
    }
}
