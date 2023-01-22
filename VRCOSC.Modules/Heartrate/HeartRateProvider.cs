// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.Heartrate;

public abstract class HeartRateProvider
{
    protected virtual string WebSocketUrl => throw new InvalidOperationException("Specify a WebSocket Url");

    private BaseWebSocket? webSocket;

    public Action? OnConnected;
    public Action? OnDisconnected;
    public Action<int>? OnHeartRateUpdate;

    public bool IsConnected { get; private set; }

    public void Initialise()
    {
        IsConnected = false;
        webSocket = new BaseWebSocket(WebSocketUrl);
        webSocket.OnWsConnected += () =>
        {
            IsConnected = true;
            HandleWsConnected();
            OnConnected?.Invoke();
        };
        webSocket.OnWsDisconnected += () =>
        {
            IsConnected = false;
            HandleWsDisconnected();
            OnDisconnected?.Invoke();
        };
        webSocket.OnWsMessage += HandleWsMessage;
    }

    public void Connect()
    {
        if (webSocket is null) throw new InvalidOperationException("Please call Initialise first");

        webSocket.Connect();
    }

    public void Disconnect()
    {
        if (webSocket is null) throw new InvalidOperationException("Please call Initialise first");

        webSocket.Disconnect();
    }

    protected void SendData(object data)
    {
        if (webSocket is null) throw new InvalidOperationException("Please call Initialise first");

        webSocket.Send(JsonConvert.SerializeObject(data));
    }

    protected virtual void HandleWsConnected() { }
    protected virtual void HandleWsDisconnected() { }
    protected virtual void HandleWsMessage(string message) { }
}
