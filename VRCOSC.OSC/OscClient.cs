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
    public void SendValue(string oscAddress, object value) => SendValues(oscAddress, new List<object> { value });

    /// <summary>
    /// Sends values to a specified address
    /// </summary>
    /// <param name="oscAddress">The address to send the value to</param>
    /// <param name="values">The values to send</param>
    /// <exception cref="ArgumentOutOfRangeException">If the values are not of type bool, int, float, or string</exception>
    public void SendValues(string oscAddress, List<object> values)
    {
        if (!values.All(value => value is (bool or int or float or string)))
            throw new ArgumentOutOfRangeException(nameof(values), "Cannot send values that are not of type bool, int, float, or string");

        sendingClient?.SendOscMessage(new OscMessage(oscAddress, values));
        OnParameterSent?.Invoke(oscAddress, values.First());
    }

    private async void runReceiveLoop()
    {
        try
        {
            while (!tokenSource!.Token.IsCancellationRequested)
            {
                var message = await receivingClient!.ReceiveOscMessageAsync(tokenSource.Token);
                if (message is null || !message.Arguments.Any()) continue;

                OnParameterReceived?.Invoke(message.Address, message.Arguments.First()!);
            }
        }
        catch (OperationCanceledException) { }
    }
}
