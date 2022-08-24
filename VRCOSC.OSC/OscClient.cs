// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using System.Net.Sockets;
using CoreOSC;

namespace VRCOSC.OSC;

public class OscClient
{
    private Socket? sendingClient;
    private Socket? receivingClient;
    private CancellationTokenSource? tokenSource;
    private Task? incomingTask;
    private bool sendingEnabled;
    private bool receivingEnabled;

    public Action<string, object>? OnParameterSent;
    public Action<string, object>? OnParameterReceived;

    /// <summary>
    /// Initialises the <see cref="OscClient"/> with the required data.
    /// </summary>
    /// <remarks>This can be called multiple times without having to make a new <see cref="OscClient"/> as long as the <see cref="OscClient"/> is disabled</remarks>
    /// <param name="ipAddress">The IP address which which to send and receive data on</param>
    /// <param name="sendPort">The port with which to send data</param>
    /// <param name="receivePort">The port with which to receive data</param>
    /// <exception cref="InvalidOperationException">If the <see cref="OscClient"/> is currently enabled</exception>
    public void Initialise(string ipAddress, int sendPort, int receivePort)
    {
        if (sendingEnabled || receivingEnabled) throw new InvalidOperationException($"Cannot initialise when {nameof(OscClient)} is already enabled");

        sendingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        receivingClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        sendingClient.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), sendPort));
        receivingClient.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), receivePort));
    }

    /// <summary>
    /// Enables the <see cref="OscClient"/> to start sending and receiving data
    /// </summary>
    public void Enable()
    {
        tokenSource = new CancellationTokenSource();
        incomingTask = Task.Run(runReceiveLoop);
        sendingEnabled = true;
        receivingEnabled = true;
    }

    /// <summary>
    /// Disables both sending and receiving
    /// </summary>
    public async Task Disable()
    {
        await DisableReceive();
        DisableSend();
    }

    /// <summary>
    /// Disables just receiving
    /// </summary>
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

    /// <summary>
    /// Disables just sending
    /// </summary>
    public void DisableSend()
    {
        sendingEnabled = false;
        sendingClient?.Close();
        sendingClient = null;
    }

    /// <summary>
    /// Sends a value to a specified address
    /// </summary>
    /// <param name="oscAddress">The address to send the value to</param>
    /// <param name="value">The value to send</param>
    /// <exception cref="ArgumentOutOfRangeException">If the value is not of type bool, int, float, or string</exception>
    public void SendValue<T>(string oscAddress, T value)
    {
        if (value is not (bool or int or float or string))
            throw new ArgumentOutOfRangeException(nameof(value), "Cannot send value that is not of type bool, int, float, or string");

        sendingClient?.SendOscMessage(new OscMessage(new Address(oscAddress), new[] { primitiveToOsc(value) }));
        OnParameterSent?.Invoke(oscAddress, value);
    }

    private async void runReceiveLoop()
    {
        try
        {
            while (!tokenSource!.Token.IsCancellationRequested)
            {
                var message = await receivingClient!.ReceiveOscMessageAsync(tokenSource.Token);

                // ensure there is always some data
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
