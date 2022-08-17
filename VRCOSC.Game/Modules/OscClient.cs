// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreOSC;

namespace VRCOSC.Game.Modules;

public class OscClient
{
    private Socket? sendingClient;
    private Socket? receivingClient;
    private CancellationTokenSource? tokenSource;
    private Task? incomingTask;

    public Action<string, object>? OnParameterSent;
    public Action<string, object>? OnParameterReceived;

    public void Initialise(string ipAddress, int sendPort, int receivePort)
    {
        sendingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receivingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        sendingClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), sendPort));
        receivingClient.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), receivePort));
    }

    public void Enable()
    {
        tokenSource = new CancellationTokenSource();
        incomingTask = Task.Run(listenForIncoming);
    }

    public async Task DisableReceive()
    {
        tokenSource?.Cancel();
        if (incomingTask is not null) await incomingTask;
        tokenSource?.Dispose();
        receivingClient?.Close();
        receivingClient = null;
    }

    public void DisableSend()
    {
        sendingClient?.Close();
        sendingClient = null;
    }

    public void SendData(string address, bool value) => sendData(address, value ? OscTrue.True : OscFalse.False);

    public void SendData(string address, int value) => sendData(address, value);

    public void SendData(string address, float value) => sendData(address, value);

    private void sendData(string address, object value)
    {
        var oscAddress = new Address(address);
        var message = new OscMessage(oscAddress, new[] { value });

        if (sendingClient is null) return;

        sendingClient.SendOscMessage(message);
        OnParameterSent?.Invoke(address, convertValue(value));
    }

    private async void listenForIncoming()
    {
        try
        {
            while (!tokenSource!.Token.IsCancellationRequested)
            {
                var message = await receivingClient!.ReceiveOscMessage(tokenSource.Token);
                if (!message.Arguments.Any()) continue;

                OnParameterReceived?.Invoke(message.Address.Value, convertValue(message.Arguments.First()));
            }
        }
        catch (OperationCanceledException) { }
    }

    private object convertValue(object value)
    {
        if (value is OscTrue) value = true;
        if (value is OscFalse) value = false;
        return value;
    }
}
