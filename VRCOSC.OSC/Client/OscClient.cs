// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;

namespace VRCOSC.OSC.Client;

public abstract class OscClient
{
    private Socket? sendingClient;
    private Socket? receivingClient;
    private CancellationTokenSource? tokenSource;
    private Task? incomingTask;
    private bool sendingEnabled;
    private bool receivingEnabled;

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

        await (incomingTask ?? Task.CompletedTask);

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
    public void SendValues(string address, List<object> values) => sendData(new OscData(address, values));

    private void sendData(OscData data)
    {
        data.PreValidate();
        sendingClient?.SendOscMessage(new OscMessage(data.Address, data.Values));
        OnDataSend(data);
    }

    private async void runReceiveLoop()
    {
        while (true)
        {
            try
            {
                var message = await receivingClient!.ReceiveOscMessageAsync(tokenSource!.Token);
                if (message is null) continue;

                OnDataReceived(new OscData(message.Address, message.Values));
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    protected abstract void OnDataSend(OscData data);
    protected abstract void OnDataReceived(OscData data);
}