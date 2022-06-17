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
    private const string osc_ip_address = "127.0.0.1";
    private const int osc_send_port = 9000;
    private const int osc_receive_port = 9001;

    private readonly UdpClient sendingClient;
    private readonly UdpClient receivingClient;

    private readonly CancellationTokenSource tokenSource = new();

    public Action<string, object>? OnParameterSent;
    public Action<string, object>? OnParameterReceived;

    public OscClient()
    {
        sendingClient = new UdpClient(osc_ip_address, osc_send_port);
        var receiveEndpoint = new IPEndPoint(IPAddress.Parse(osc_ip_address), osc_receive_port);
        receivingClient = new UdpClient(receiveEndpoint);
    }

    public void Enable()
    {
        Task.Run(listenForIncoming);
    }

    public void Disable()
    {
        tokenSource.Cancel();
        tokenSource.TryReset();
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

        while (!tokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var message = await receivingClient.ReceiveMessageAsync();
                if (!message.Arguments.Any()) continue;

                OnParameterReceived?.Invoke(message.Address.Value, message.Arguments.First());
            }
            catch (SocketException _) { }
        }
    }

    public void Dispose()
    {
        sendingClient.Dispose();
        receivingClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
