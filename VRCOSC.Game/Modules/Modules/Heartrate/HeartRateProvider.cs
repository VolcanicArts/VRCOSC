using System;
using System.Threading.Tasks;
using VRCOSC.Game.Modules.Util;
using VRCOSC.Game.Modules.Websocket;

namespace VRCOSC.Game.Modules.Modules.Heartrate;

public abstract class HeartRateProvider
{
    protected virtual string WebSocketUrl => throw new InvalidOperationException("Specify a WebSocket Url");
    protected virtual int WebSocketHeartBeat => throw new InvalidOperationException("Specify a WebSocket heartbeat");
    protected virtual bool SendWsHeartBeat => true;

    private JsonWebSocket? webSocket;
    private TimedTask? wsHeartBeatTask;

    public Action? OnConnected;
    public Action? OnDisconnected;
    public Action? OnWsHeartBeat;
    public Action<int>? OnHeartRateUpdate;

    public void Initialise()
    {
        webSocket = new JsonWebSocket(WebSocketUrl);
        webSocket.OnWsConnected += () =>
        {
            HandleWsConnected();
            OnConnected?.Invoke();
        };
        webSocket.OnWsDisconnected += () =>
        {
            HandleWsDisconnected();
            OnDisconnected?.Invoke();
        };
        webSocket.OnWsMessage += HandleWsMessage;

        wsHeartBeatTask = new TimedTask(() =>
        {
            HandleWsHeartBeat();
            OnWsHeartBeat?.Invoke();
        }, WebSocketHeartBeat);
    }

    public void Connect()
    {
        if (webSocket is null || wsHeartBeatTask is null) throw new InvalidOperationException("Please call Initialise first");

        webSocket.Connect();
        if (SendWsHeartBeat) wsHeartBeatTask.Start();
    }

    public async Task Disconnect()
    {
        if (webSocket is null || wsHeartBeatTask is null) throw new InvalidOperationException("Please call Initialise first");

        webSocket.Disconnect();
        await wsHeartBeatTask.Stop();
    }

    protected void SendData(object data)
    {
        if (webSocket is null) throw new InvalidOperationException("Please call Initialise first");

        webSocket.SendAsJson(data);
    }

    protected virtual void HandleWsConnected() { }
    protected virtual void HandleWsDisconnected() { }
    protected virtual void HandleWsMessage(string message) { }
    protected virtual void HandleWsHeartBeat() { }
}
