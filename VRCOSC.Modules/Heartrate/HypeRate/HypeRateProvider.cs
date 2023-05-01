// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.Modules;
using VRCOSC.Modules.Heartrate.HypeRate.Models;

namespace VRCOSC.Modules.Heartrate.HypeRate;

public sealed class HypeRateProvider : HeartRateProvider
{
    private readonly string hypeRateId;
    private readonly string apiKey;

    protected override string WebSocketUrl => $"wss://app.hyperate.io/socket/websocket?token={apiKey}";

    public HypeRateProvider(string hypeRateId, string apiKey, TerminalLogger terminal)
        : base(terminal)
    {
        this.hypeRateId = hypeRateId;
        this.apiKey = apiKey;
    }

    protected override void HandleWsConnected()
    {
        Log("Successfully connected to the HypeRate websocket");
        sendJoinChannel();
    }

    protected override void HandleWsDisconnected()
    {
        Log("Disconnected from the HypeRate websocket");
    }

    protected override void HandleWsMessage(string message)
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

            case "phx_reply":
                handlePhxReply(JsonConvert.DeserializeObject<PhxReplyModel>(message)!);
                break;
        }
    }

    public void SendWsHeartBeat()
    {
        Log("Sending HypeRate websocket heartbeat");
        SendData(new HeartBeatModel());
    }

    private void sendJoinChannel()
    {
        Log($"Requesting to hook into heartrate for Id {hypeRateId}");

        var joinChannelModel = new JoinChannelModel
        {
            Id = hypeRateId
        };
        SendData(joinChannelModel);
    }

    private void handlePhxReply(PhxReplyModel reply)
    {
        Log($"Status of reply: {reply.Payload.Status}");
    }

    private void handleHrUpdate(HeartRateUpdateModel update)
    {
        OnHeartRateUpdate?.Invoke(update.Payload.HeartRate);
    }
}
