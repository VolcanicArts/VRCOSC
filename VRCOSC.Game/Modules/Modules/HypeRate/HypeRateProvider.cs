// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using Newtonsoft.Json;
using VRCOSC.Game.Modules.Frameworks;
using VRCOSC.Game.Modules.Modules.HypeRate.Models;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Modules.HypeRate;

public class HypeRateProvider : JsonWebSocket
{
    private const string hype_rate_uri = "wss://app.hyperate.io/socket/websocket?token=";
    private const int heartbeat_internal = 10000;

    private readonly string hyperateId;
    private readonly TerminalLogger terminal = new(nameof(HypeRateModule));

    private Timer? heartBeatTimer;

    public Action<int>? OnHeartRateUpdate;
    public Action? OnWsHeartbeat;

    public HypeRateProvider(string hyperateId, string apiKey)
        : base(hype_rate_uri + apiKey)
    {
        this.hyperateId = hyperateId;

        OnWsConnected += OnConnected;
        OnWsDisconnected += OnDisconnected;
        OnWsMessage += OnMessage;
    }

    private void OnConnected()
    {
        terminal.Log("Successfully connected to the HypeRate websocket");
        sendJoinChannel();
        initHeartBeat();
    }

    private void OnDisconnected()
    {
        terminal.Log("Disconnected from the HypeRate websocket");
        Dispose();
    }

    private void OnMessage(string message)
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
        heartBeatTimer = new Timer(sendHeartBeat, null, heartbeat_internal, 30000);
    }

    private void sendHeartBeat(object? _)
    {
        terminal.Log("Sending HypeRate websocket heartbeat");
        SendAsJson(new HeartBeatModel());
        OnWsHeartbeat?.Invoke();
        heartBeatTimer?.Change(heartbeat_internal, 30000);
    }

    private void sendJoinChannel()
    {
        terminal.Log($"Requesting to hook into heartrate for Id {hyperateId}");
        var joinChannelModel = new JoinChannelModel
        {
            Id = hyperateId
        };
        SendAsJson(joinChannelModel);
    }

    private void handlePhxReply(PhxReplyModel reply)
    {
        terminal.Log($"Status of reply: {reply.Payload.Status}");
    }

    private void handleHrUpdate(HeartRateUpdateModel update)
    {
        OnHeartRateUpdate?.Invoke(update.Payload.HeartRate);
    }

    public override void Dispose()
    {
        heartBeatTimer?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
