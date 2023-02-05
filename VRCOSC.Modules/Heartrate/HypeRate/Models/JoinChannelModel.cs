// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace VRCOSC.Modules.Heartrate.HypeRate.Models;

public sealed class JoinChannelModel
{
    [JsonProperty("event")]
    public string Event = "phx_join";

    [JsonProperty("payload")]
    public JoinChannelPayload Payload = new();

    [JsonProperty("ref")]
    public int Ref;

    [JsonProperty("topic")]
    public string topic = null!;

    [JsonIgnore]
    public string Id
    {
        set => topic = "hr:" + value;
    }
}

public sealed class JoinChannelPayload
{
}
