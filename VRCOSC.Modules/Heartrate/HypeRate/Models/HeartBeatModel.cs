// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace VRCOSC.Modules.Heartrate.HypeRate.Models;

public sealed class HeartBeatModel
{
    [JsonProperty("event")]
    public string Event = "heartbeat";

    [JsonProperty("payload")]
    public WebSocketHeartBeatPayload Payload = new();

    [JsonProperty("ref")]
    public int Ref;

    [JsonProperty("topic")]
    public string Topic = "phoenix";
}

public sealed class WebSocketHeartBeatPayload
{
}
