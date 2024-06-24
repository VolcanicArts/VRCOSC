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

    public Action? OnWsConnected;
    public Action? OnWsDisconnected;
    public Action<string>? OnWsMessage;
    public bool IsConnected => webSocket.State == WebSocketState.Open;

    private readonly Uri uri;

    public WebSocketClient(string uri)
    {
        this.uri = new Uri(uri);
    }

    public async Task ConnectAsync()
    {
        try
        {
            await webSocket.ConnectAsync(uri, tokenSource.Token);
            if (!IsConnected) return;

            OnWsConnected?.Invoke();
            _ = receiveAsync();
        }
        catch (Exception e)
        {
            Logger.Error(e, "WebSocketClient connection failed");
        }
    }

    public async Task DisconnectAsync()
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        await tokenSource.CancelAsync();
    }

    public async Task SendAsync(string data)
    {
        var buffer = Encoding.UTF8.GetBytes(data);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, tokenSource.Token);
    }

    private async Task receiveAsync()
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            var message = new StringBuilder();

            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), tokenSource.Token);
                message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            OnWsMessage?.Invoke(message.ToString());
        }

        OnWsDisconnected?.Invoke();
    }

    public virtual void Dispose()
    {
        webSocket.Dispose();
        tokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}
