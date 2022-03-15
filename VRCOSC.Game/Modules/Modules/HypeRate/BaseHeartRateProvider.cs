using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Logging;
using SuperSocket.ClientEngine;
using WebSocket4Net;

namespace VRCOSC.Game.Modules.Modules;

public abstract class BaseHeartRateProvider
{
    private readonly EventWaitHandle IsRunning = new AutoResetEvent(false);
    protected readonly Logger Terminal = Logger.GetLogger("terminal");

    private readonly WebSocket WebSocket;
    public Action? OnConnected;
    public Action? OnDisconnected;
    public Action<int>? OnHeartRateUpdate;

    protected BaseHeartRateProvider(string Uri)
    {
        Terminal.Add("Creating base websocket", LogLevel.Debug);
        WebSocket = new WebSocket(Uri);
        WebSocket.Opened += wsConnected;
        WebSocket.Closed += wsDisconnected;
        WebSocket.MessageReceived += wsMessageReceived;
        WebSocket.Error += wsError;
    }

    public void Connect()
    {
        Task.Factory.StartNew(run, TaskCreationOptions.LongRunning);
    }

    public void Disconnect()
    {
        IsRunning.Set();
        WebSocket.Close();
    }

    protected void Send(object data)
    {
        WebSocket.Send(JsonConvert.SerializeObject(data));
    }

    private void run()
    {
        WebSocket.Open();
        IsRunning.WaitOne();
    }

    private void wsConnected(object? sender, EventArgs e)
    {
        Terminal.Add("WebSocket successfully connected", LogLevel.Debug);
        OnConnected?.Invoke();
        OnWsConnected();
    }

    protected abstract void OnWsConnected();

    private void wsDisconnected(object? sender, EventArgs e)
    {
        Terminal.Add("WebSocket disconnected", LogLevel.Debug);
        OnDisconnected?.Invoke();
        OnWsDisconnected();
        IsRunning.Set();
    }

    protected abstract void OnWsDisconnected();

    private void wsMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        Terminal.Add(e.Message, LogLevel.Debug);
        OnWsMessageReceived(e.Message);
    }

    protected abstract void OnWsMessageReceived(string message);

    private void wsError(object? sender, ErrorEventArgs e)
    {
        Terminal.Add(e.Exception.ToString());
    }
}
