// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.Game.OSC.Client;

public class OscReceiver
{
    private Socket? socket;
    private IPEndPoint endPoint = null!;
    private CancellationTokenSource? tokenSource;
    private Task? incomingTask;

    public Action<byte[]>? OnRawDataReceived;

    private readonly byte[] buffer = new byte[4096];

    public void Initialise(IPEndPoint endPoint)
    {
        this.endPoint = endPoint;
    }

    public void Enable()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(endPoint);

        tokenSource = new CancellationTokenSource();
        incomingTask = Task.Run(runReceiveLoop);
    }

    public async Task Disable()
    {
        tokenSource?.Cancel();

        await (incomingTask ?? Task.CompletedTask);

        incomingTask?.Dispose();
        tokenSource?.Dispose();
        socket?.Close();

        incomingTask = null;
        tokenSource = null;
        socket = null;
    }

    private async void runReceiveLoop()
    {
        while (!tokenSource!.Token.IsCancellationRequested)
        {
            try
            {
                buffer.Initialize();
                await socket!.ReceiveAsync(buffer, SocketFlags.None, tokenSource.Token);
                OnRawDataReceived?.Invoke(buffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
