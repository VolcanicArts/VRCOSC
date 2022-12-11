// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.Heartrate;

public abstract class HeartRateProvider
{
    protected virtual string WebSocketUrl => throw new InvalidOperationException("Specify a WebSocket Url");
    protected virtual int WebSocketHeartBeat => int.MaxValue;
    protected virtual bool SendWsHeartBeat => true;

    private BaseWebSocket? webSocket;
    private TimedTask? wsHeartBeatTask;

    public Action? OnConnected;
    public Action? OnDisconnected;
    public Action? OnWsHeartBeat;
    public Action<int>? OnHeartRateUpdate;

    public void Initialise()
    {
        webSocket = new BaseWebSocket(WebSocketUrl);
        webSocket.OnWsConnected += () =>
        {
            HandleWsConnected();
            OnConnected?.Invoke();
        };
        webSocket.OnWsDisconnected += () =>
        {
            HandleWsDisconnected();
            OnDisconnected?.Invoke();
            wsHeartBeatTask?.Stop();
        };
        webSocket.OnWsMessage += HandleWsMessage;

        wsHeartBeatTask = new TimedTask(() =>
        {
            HandleWsHeartBeat();
            OnWsHeartBeat?.Invoke();
            return Task.CompletedTask;
        }, WebSocketHeartBeat);
    }

    public void Connect()
    {
        if (webSocket is null || wsHeartBeatTask is null) throw new InvalidOperationException("Please call Initialise first");

        webSocket.Connect();
        if (SendWsHeartBeat) _ = wsHeartBeatTask.Start();
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

        webSocket.Send(JsonConvert.SerializeObject(data));
    }

    protected virtual void HandleWsConnected() { }
    protected virtual void HandleWsDisconnected() { }
    protected virtual void HandleWsMessage(string message) { }
    protected virtual void HandleWsHeartBeat() { }
}
