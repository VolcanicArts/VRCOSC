// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ClientEngine;
using VRCOSC.Game.Util;
using WebSocket4Net;

// ReSharper disable MemberCanBeProtected.Global

namespace VRCOSC.Game.Modules.Websocket;

public class BaseWebSocket : IDisposable
{
    private readonly CancellationTokenSource tokenSource = new();
    private readonly TerminalLogger terminal = new(nameof(BaseWebSocket));
    private readonly WebSocket webSocket;

    public Action? OnWsConnected;
    public Action? OnWsDisconnected;
    public Action<string>? OnWsMessage;

    public BaseWebSocket(string uri)
    {
        terminal.Log("Creating base websocket");
        webSocket = new WebSocket(uri);
        webSocket.Opened += wsConnected;
        webSocket.Closed += wsDisconnected;
        webSocket.MessageReceived += wsMessageReceived;
        webSocket.Error += wsError;
    }

    public void Connect()
    {
        Task.Run(() =>
        {
            webSocket.Open();
            tokenSource.Token.WaitHandle.WaitOne();
        });
    }

    public void Disconnect()
    {
        webSocket.Close();
    }

    public void Send(string data)
    {
        webSocket.Send(data);
    }

    private void wsConnected(object? sender, EventArgs e)
    {
        terminal.Log("WebSocket successfully connected");
        OnWsConnected?.Invoke();
    }

    private void wsDisconnected(object? sender, EventArgs e)
    {
        terminal.Log("WebSocket disconnected");
        OnWsDisconnected?.Invoke();
        tokenSource.Cancel();
    }

    private void wsMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        OnWsMessage?.Invoke(e.Message);
    }

    private void wsError(object? sender, ErrorEventArgs e)
    {
        terminal.Log(e.Exception.ToString());
    }

    public virtual void Dispose()
    {
        webSocket.Dispose();
        GC.SuppressFinalize(this);
    }
}
