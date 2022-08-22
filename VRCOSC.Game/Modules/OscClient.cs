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
        incomingTask = Task.Run(runReceiveLoop);
    }

    public async Task DisableReceive()
    {
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
        sendingClient?.Close();
        sendingClient = null;
    }

    public void SendValue<T>(string oscAddress, T value) where T : struct
    {
        if (value is not (bool or int or float))
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot send value that is not of type bool, int, or float");

        sendingClient?.SendOscMessage(new OscMessage(new Address(oscAddress), new[] { primitiveToOsc(value) }));
        OnParameterSent?.Invoke(oscAddress, value);
    }

    private async void runReceiveLoop()
    {
        try
        {
            while (!tokenSource!.Token.IsCancellationRequested)
            {
                var message = await receivingClient!.ReceiveOscMessage(tokenSource.Token);

                // if values arrive too fast, vrc can occasionally send data without a value
                if (!message.Arguments.Any()) continue;

                OnParameterReceived?.Invoke(message.Address.Value, oscToPrimitive(message.Arguments.First()));
            }
        }
        catch (OperationCanceledException) { }
    }

    private static object primitiveToOsc(object value)
    {
        if (value is bool boolValue) return boolValue ? OscTrue.True : OscFalse.False;

        return value;
    }

    private static object oscToPrimitive(object value)
    {
        return value switch
        {
            OscTrue => true,
            OscFalse => false,
            _ => value
        };
    }
}
