using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.Models;

public class HeartBeatModel
{
    [JsonProperty("event")]
    private string Event = "heartbeat";

    [JsonProperty("payload")]
    private WebSocketHeartBeatPayload Payload = new();

    [JsonProperty("ref")]
    private int Ref;

    [JsonProperty("topic")]
    private string Topic = "phoenix";
}

public class WebSocketHeartBeatPayload { }
