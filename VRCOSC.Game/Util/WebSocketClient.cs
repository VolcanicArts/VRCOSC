// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace VRCOSC.Game.Util;

public class WebSocketClient : IDisposable
{
    private readonly CancellationTokenSource tokenSource = new();
    private readonly WebSocket webSocket;

    public Action? OnWsConnected;
    public Action? OnWsDisconnected;
    public Action<string>? OnWsMessage;
    public bool IsConnected => webSocket.Handshaked;

    public WebSocketClient(string uri)
    {
        webSocket = new WebSocket(uri);
        webSocket.Opened += wsConnected;
        webSocket.Closed += wsDisconnected;
        webSocket.MessageReceived += wsMessageReceived;
    }

    public void Connect()
    {
        Task.Run(() =>
        {
            webSocket.Open();
            tokenSource.Token.WaitHandle.WaitOne();
        });
    }

    public Task<bool> Disconnect()
    {
        return webSocket.CloseAsync();
    }

    public void Send(string data)
    {
        webSocket.Send(data);
    }

    private void wsConnected(object? sender, EventArgs e)
    {
        OnWsConnected?.Invoke();
    }

    private void wsDisconnected(object? sender, EventArgs e)
    {
        OnWsDisconnected?.Invoke();
        tokenSource.Cancel();
    }

    private void wsMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        OnWsMessage?.Invoke(e.Message);
    }

    public virtual void Dispose()
    {
        webSocket.Dispose();
        GC.SuppressFinalize(this);
    }
}
