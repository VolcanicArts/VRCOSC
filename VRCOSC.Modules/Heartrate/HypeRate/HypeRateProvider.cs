// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.Modules.Bases.Heartrate;
using VRCOSC.Modules.Heartrate.HypeRate.Models;

namespace VRCOSC.Modules.Heartrate.HypeRate;

public sealed class HypeRateProvider : WebSocketHeartrateProvider
{
    private readonly string hypeRateId;
    private readonly string apiKey;

    protected override Uri WebsocketUri => new($"wss://app.hyperate.io/socket/websocket?token={apiKey}");

    public HypeRateProvider(string hypeRateId, string apiKey)
    {
        this.hypeRateId = hypeRateId;
        this.apiKey = apiKey;
    }

    protected override void OnWebSocketConnected()
    {
        Log("Connected to the HypeRate websocket");
        sendJoinChannel();
    }

    protected override void OnWebSocketDisconnected()
    {
        Log("Disconnected from the HypeRate websocket");
    }

    protected override void OnWebSocketMessage(string message)
    {
        try
        {
            var eventModel = JsonConvert.DeserializeObject<EventModel>(message);

            if (eventModel is null)
            {
                Log($"Received an unrecognised message:\n{message}");
                return;
            }

            switch (eventModel.Event)
            {
                case "hr_update":
                    handleHrUpdate(JsonConvert.DeserializeObject<HeartRateUpdateModel>(message)!);
                    break;
            }
        }
        catch (JsonReaderException)
        {
            Log("Error receiving heartrate result");
        }
    }

    public void SendWsHeartBeat()
    {
        SendDataAsJson(new HeartBeatModel());
    }

    private void sendJoinChannel()
    {
        var joinChannelModel = new JoinChannelModel
        {
            Id = hypeRateId
        };
        SendDataAsJson(joinChannelModel);
    }

    private void handleHrUpdate(HeartRateUpdateModel update)
    {
        OnHeartrateUpdate?.Invoke(update.Payload.HeartRate);
    }
}
