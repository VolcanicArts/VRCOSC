// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperSocket.ClientEngine;
using VRCOSC.Game.Util;
using WebSocket4Net;

namespace VRCOSC.Game.Modules.Modules;

public abstract class BaseHeartRateProvider
{
    private readonly EventWaitHandle isRunning = new AutoResetEvent(false);
    private readonly TerminalLogger terminal = new("HypeRateModule");
    private readonly WebSocket webSocket;

    public Action? OnConnected;
    public Action? OnDisconnected;
    public Action<int>? OnHeartRateUpdate;

    protected BaseHeartRateProvider(string uri)
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
        Task.Factory.StartNew(run, TaskCreationOptions.LongRunning);
    }

    public void Disconnect()
    {
        isRunning.Set();
        webSocket.Close();
    }

    protected void Send(object data)
    {
        webSocket.Send(JsonConvert.SerializeObject(data));
    }

    private void run()
    {
        webSocket.Open();
        isRunning.WaitOne();
    }

    private void wsConnected(object? sender, EventArgs e)
    {
        terminal.Log("WebSocket successfully connected");
        OnConnected?.Invoke();
        OnWsConnected();
    }

    protected abstract void OnWsConnected();

    private void wsDisconnected(object? sender, EventArgs e)
    {
        terminal.Log("WebSocket disconnected");
        OnDisconnected?.Invoke();
        OnWsDisconnected();
        isRunning.Set();
    }

    protected abstract void OnWsDisconnected();

    private void wsMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        OnWsMessageReceived(e.Message);
    }

    protected abstract void OnWsMessageReceived(string message);

    private void wsError(object? sender, ErrorEventArgs e)
    {
        terminal.Log(e.Exception.ToString());
    }
}
