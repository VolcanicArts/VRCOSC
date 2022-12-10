// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;

namespace VRCOSC.OSC;

public sealed class OscClient
{
    private Socket? sendingClient;
    private Socket? receivingClient;
    private CancellationTokenSource? tokenSource;
    private Task? incomingTask;
    private bool sendingEnabled;
    private bool receivingEnabled;

    private readonly List<IOscListener> listeners = new();

    public void RegisterListener(IOscListener listener)
    {
        listeners.Add(listener);
    }

    public void DeRegisterListener(IOscListener listener)
    {
        listeners.Remove(listener);
    }

    public void Initialise(string ipAddress, int sendPort, int receivePort)
    {
        if (sendingEnabled || receivingEnabled) throw new InvalidOperationException($"Cannot initialise when {nameof(OscClient)} is already enabled");

        sendingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receivingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        sendingClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), sendPort));
        receivingClient.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), receivePort));
    }

    public void Enable()
    {
        tokenSource = new CancellationTokenSource();
        incomingTask = Task.Run(runReceiveLoop);
        sendingEnabled = true;
        receivingEnabled = true;
    }

    public async Task DisableReceive()
    {
        receivingEnabled = false;
        tokenSource?.Cancel();

        if (incomingTask is not null) await incomingTask;

        incomingTask?.Dispose();
        tokenSource?.Dispose();
        receivingClient?.Close();

        incomingTask = null;
        tokenSource = null;
        receivingClient = null;
    }

    public void DisableSend()
    {
        sendingEnabled = false;
        sendingClient?.Close();
        sendingClient = null;
    }

    public void SendValue(string address, object value) => SendValues(address, new List<object> { value });

    public void SendValues(string address, List<object> values) => SendData(new OscData
    {
        Address = address,
        Values = values
    });

    public void SendData(OscData data)
    {
        data.PreValidate();
        sendingClient?.SendOscMessage(new OscMessage(data.Address, data.Values));
        listeners.ForEach(listener => listener.OnDataSent(data));
    }

    private async void runReceiveLoop()
    {
        try
        {
            while (!tokenSource!.Token.IsCancellationRequested)
            {
                var message = await receivingClient!.ReceiveOscMessageAsync(tokenSource.Token);
                if (message is null) continue;

                var data = new OscData
                {
                    Address = message.Address,
                    Values = message.Values
                };

                listeners.ForEach(listener => listener.OnDataReceived(data));
            }
        }
        catch (OperationCanceledException) { }
    }
}
