// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreOSC;
using CoreOSC.IO;

namespace VRCOSC.Game.Modules;

public class OscClient : IDisposable
{
    private UdpClient? sendingClient;
    private UdpClient? receivingClient;

    private CancellationTokenSource? tokenSource;

    public Action<string, object>? OnParameterSent;
    public Action<string, object>? OnParameterReceived;

    public void Initialise(string ipAddress, int sendPort, int receivePort)
    {
        sendingClient?.Dispose();
        sendingClient = new UdpClient(ipAddress, sendPort);

        receivingClient?.Dispose();
        receivingClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ipAddress), receivePort));
    }

    public void Enable()
    {
        tokenSource = new CancellationTokenSource();
        Task.Run(listenForIncoming);
    }

    public void Disable()
    {
        tokenSource?.Cancel();
    }

    public void SendData(string address, bool value) => sendData(address, value ? OscTrue.True : OscFalse.False);

    public void SendData(string address, int value) => sendData(address, value);

    public void SendData(string address, float value) => sendData(address, value);

    private void sendData(string address, object value)
    {
        var oscAddress = new Address(address);
        var message = new OscMessage(oscAddress, new[] { value });
        sendingClient.SendMessageAsync(message);

        OnParameterSent?.Invoke(address, value);
    }

    private async void listenForIncoming()
    {
        if (tokenSource == null) throw new AggregateException("Cancellation token is null when trying to listen for OSC messages");

        try
        {
            while (!tokenSource.Token.IsCancellationRequested)
            {
                var message = await receivingClient.ReceiveMessageAsync().WaitAsync(tokenSource.Token);
                if (!message.Arguments.Any()) continue;

                OnParameterReceived?.Invoke(message.Address.Value, message.Arguments.First());
            }
        }
        // required due to CoreOSC not handling cancellation requests
        catch (TaskCanceledException) { }
    }

    public void Dispose()
    {
        sendingClient?.Dispose();
        receivingClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
