// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using Newtonsoft.Json;
using VRCOSC.Game.Modules.Modules.Models;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Modules;

public class HypeRateProvider : HeartrateProvider
{
    private const string hype_rate_uri = "wss://app.hyperate.io/socket/websocket?token=";
    private const int heartbeat_internal = 30000;
    private readonly string id;
    private Timer? heartBeatTimer;
    private readonly TerminalLogger terminal = new(nameof(HypeRateModule));

    public HypeRateProvider(string id, string apiKey)
        : base(hype_rate_uri + apiKey)
    {
        this.id = id;
    }

    protected override void OnWsConnected()
    {
        terminal.Log("Successfully connected to the HypeRate websocket");
        sendJoinChannel();
        initHeartBeat();
    }

    protected override void OnWsDisconnected()
    {
        terminal.Log("Disconnected from the HypeRate websocket");
        Dispose();
    }

    protected override void OnWsMessageReceived(string message)
    {
        var eventModel = JsonConvert.DeserializeObject<EventModel>(message);

        if (eventModel == null)
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

    private void initHeartBeat()
    {
        heartBeatTimer = new Timer(sendHeartBeat, null, heartbeat_internal, Timeout.Infinite);
    }

    private void sendHeartBeat(object? _)
    {
        terminal.Log("Sending HypeRate websocket heartbeat");
        Send(new HeartBeatModel());
        heartBeatTimer?.Change(heartbeat_internal, Timeout.Infinite);
    }

    private void sendJoinChannel()
    {
        terminal.Log($"Requesting to hook into heartrate for Id {id}");
        var joinChannelModel = new JoinChannelModel
        {
            Id = id
        };
        Send(joinChannelModel);
    }

    private void handlePhxReply(PhxReplyModel reply)
    {
        terminal.Log($"Status of reply: {reply.Payload.Status}");
    }

    private void handleHrUpdate(HeartRateUpdateModel update)
    {
        var heartRate = update.Payload.HeartRate;
        terminal.Log($"Received heartrate {heartRate}");
        OnHeartRateUpdate?.Invoke(heartRate);
    }

    public override void Dispose()
    {
        base.Dispose();
        heartBeatTimer?.Dispose();
    }
}
