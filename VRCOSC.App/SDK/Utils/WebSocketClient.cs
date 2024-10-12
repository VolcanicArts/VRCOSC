// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Utils;

public class WebSocketClient : IDisposable
{
    private readonly CancellationTokenSource tokenSource = new();
    private readonly ClientWebSocket webSocket = new();
    private readonly Uri uri;
    private readonly int reconnectionDelayMilli;
    private readonly int maxReconnectAttempts;
    private bool isDisposed;
    private bool isDisconnecting;

    public Action? OnWsConnected;
    public Action? OnWsDisconnected;
    public Action<string>? OnWsMessage;
    public bool IsConnected => webSocket.State == WebSocketState.Open;

    public WebSocketClient(string uri, int reconnectionDelayMilli, int maxReconnectAttempts)
    {
        this.uri = new Uri(uri);
        this.reconnectionDelayMilli = reconnectionDelayMilli;
        this.maxReconnectAttempts = maxReconnectAttempts;
    }

    public async Task ConnectAsync()
    {
        try
        {
            await webSocket.ConnectAsync(uri, tokenSource.Token);
            OnWsConnected?.Invoke();
            _ = Task.Run(receiveAsync);
        }
        catch
        {
        }
    }

    public async Task DisconnectAsync()
    {
        isDisconnecting = true;
        await tokenSource.CancelAsync();

        if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", CancellationToken.None);
        }

        OnWsDisconnected?.Invoke();
    }

    public async Task SendAsync(string data)
    {
        if (webSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        var buffer = Encoding.UTF8.GetBytes(data);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, tokenSource.Token);
    }

    private async Task receiveAsync()
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open && !isDisconnecting)
        {
            var message = new StringBuilder();

            try
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), tokenSource.Token);
                    message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                } while (!result.EndOfMessage);

                OnWsMessage?.Invoke(message.ToString());
            }
            catch (WebSocketException e)
            {
                Logger.Error(e, $"{nameof(WebSocketClient)} has experienced an error");
                break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        if (!isDisconnecting)
        {
            OnWsDisconnected?.Invoke();
            await attemptReconnectAsync();
        }
    }

    private async Task attemptReconnectAsync()
    {
        if (isDisposed)
            return;

        var attempts = 0;

        while (!IsConnected && !isDisconnecting && attempts < maxReconnectAttempts)
        {
            await Task.Delay(reconnectionDelayMilli);
            attempts++;
            await ConnectAsync();
        }
    }

    public virtual void Dispose()
    {
        isDisposed = true;
        tokenSource.Cancel();

        if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent)
        {
            webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Dispose", CancellationToken.None).GetAwaiter().GetResult();
        }

        webSocket.Dispose();
        tokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
