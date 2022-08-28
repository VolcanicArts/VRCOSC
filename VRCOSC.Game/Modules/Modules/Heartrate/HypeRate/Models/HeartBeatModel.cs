// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules.Modules.Heartrate.HypeRate.Models;

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

public class WebSocketHeartBeatPayload
{
}
