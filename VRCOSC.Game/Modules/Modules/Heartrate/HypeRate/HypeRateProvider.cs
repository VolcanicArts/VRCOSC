// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game.Modules.Modules.Heartrate.HypeRate.Models;

namespace VRCOSC.Game.Modules.Modules.Heartrate.HypeRate;

public sealed class HypeRateProvider : HeartRateProvider
{
    private readonly string hypeRateId;
    private readonly string apiKey;
    private readonly TerminalLogger terminal = new(nameof(HypeRateModule));

    protected override string WebSocketUrl => $"wss://app.hyperate.io/socket/websocket?token={apiKey}";
    protected override int WebSocketHeartBeat => 10000;

    public HypeRateProvider(string hypeRateId, string apiKey)
    {
        this.hypeRateId = hypeRateId;
        this.apiKey = apiKey;
    }

    protected override void HandleWsConnected()
    {
        terminal.Log("Successfully connected to the HypeRate websocket");
        sendJoinChannel();
    }

    protected override void HandleWsDisconnected()
    {
        terminal.Log("Disconnected from the HypeRate websocket");
    }

    protected override void HandleWsMessage(string message)
    {
        var eventModel = JsonConvert.DeserializeObject<EventModel>(message);

        if (eventModel is null)
        {
            terminal.Log($"Received an unrecognised message:\n{message}");
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

    protected override void HandleWsHeartBeat()
    {
        terminal.Log("Sending HypeRate websocket heartbeat");
        SendData(new HeartBeatModel());
    }

    private void sendJoinChannel()
    {
        terminal.Log($"Requesting to hook into heartrate for Id {hypeRateId}");
        var joinChannelModel = new JoinChannelModel
        {
            Id = hypeRateId
        };
        SendData(joinChannelModel);
    }

    private void handlePhxReply(PhxReplyModel reply)
    {
        terminal.Log($"Status of reply: {reply.Payload.Status}");
    }

    private void handleHrUpdate(HeartRateUpdateModel update)
    {
        OnHeartRateUpdate?.Invoke(update.Payload.HeartRate);
    }
}
