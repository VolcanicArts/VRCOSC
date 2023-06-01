// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.Modules.Bases.Heartrate;
using VRCOSC.Modules.Heartrate.Pulsoid.Models;

namespace VRCOSC.Modules.Heartrate.Pulsoid;

public sealed class PulsoidProvider : WebSocketHeartrateProvider
{
    private readonly string accessToken;

    protected override Uri WebsocketUri => new($"wss://dev.pulsoid.net/api/v1/data/real_time?access_token={accessToken}");

    public PulsoidProvider(string accessToken)
    {
        this.accessToken = accessToken;
    }

    protected override void OnWebSocketConnected()
    {
        Log(@"Connected to the Pulsoid websocket");
    }

    protected override void OnWebSocketDisconnected()
    {
        Log(@"Disconnected from the Pulsoid websocket");
    }

    protected override void OnWebSocketMessage(string message)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<PulsoidResponse>(message);
            OnHeartrateUpdate?.Invoke(data!.Data.HeartRate);
        }
        catch (JsonReaderException)
        {
            Log(@"Error deserialising Pulsoid message");
        }
    }
}
