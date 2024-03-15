// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.App.SDK.Modules.Heartrate;

namespace VRCOSC.WPFModuleTest.Pulsoid;

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
        Log("Connected to the Pulsoid websocket");
    }

    protected override void OnWebSocketDisconnected()
    {
        Log("Disconnected from the Pulsoid websocket");
    }

    protected override void OnWebSocketMessage(string message)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<PulsoidResponse>(message);
            if (data is null) throw new InvalidOperationException();

            OnHeartrateUpdate?.Invoke(data.Data.HeartRate);
        }
        catch (Exception)
        {
            Log("Error deserialising Pulsoid message");
        }
    }
}

public sealed class PulsoidResponse
{
    [JsonProperty("measured_at")]
    public long MeasuredAt;

    [JsonProperty("data")]
    public PulsoidData Data = null!;
}

public sealed class PulsoidData
{
    [JsonProperty("heart_rate")]
    public int HeartRate;
}
