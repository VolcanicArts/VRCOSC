using System.Threading;
using Newtonsoft.Json;
using VRCOSC.Game.Modules.Modules.Models;

namespace VRCOSC.Game.Modules.Modules;

public class HypeRateProvider : BaseHeartRateProvider
{
    private const string hype_rate_uri = "wss://app.hyperate.io/socket/websocket?token=";
    private const int heartbeat_internal = 30000;
    private readonly string id;
    private Timer? heartBeatTimer;

    public HypeRateProvider(string id, string apiKey)
        : base(hype_rate_uri + apiKey)
    {
        this.id = id;
    }

    protected override void OnWsConnected()
    {
        Terminal.Add("Successfully connected to the HypeRate websocket");
        sendJoinChannel();
        initHeartBeat();
    }

    protected override void OnWsDisconnected()
    {
        Terminal.Add("Disconnected from the HypeRate websocket");
        heartBeatTimer?.Dispose();
    }

    protected override void OnWsMessageReceived(string message)
    {
        var eventModel = JsonConvert.DeserializeObject<EventModel>(message);

        if (eventModel == null)
        {
            Terminal.Add($"Received an unrecognised message:\n{message}");
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
        Terminal.Add("Sending HypeRate websocket heartbeat");
        Send(new HeartBeatModel());
        heartBeatTimer?.Change(heartbeat_internal, Timeout.Infinite);
    }

    private void sendJoinChannel()
    {
        Terminal.Add($"Requesting to hook into heartrate for Id {id}");
        var joinChannelModel = new JoinChannelModel
        {
            Id = id
        };
        Send(joinChannelModel);
    }

    private void handlePhxReply(PhxReplyModel reply)
    {
        Terminal.Add($"Status of reply: {reply.Payload.Status}");
    }

    private void handleHrUpdate(HeartRateUpdateModel update)
    {
        var heartRate = update.Payload.HeartRate;
        Terminal.Add($"Received heartrate {heartRate}");
        OnHeartRateUpdate?.Invoke(heartRate);
    }
}
