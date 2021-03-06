// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.HypeRate.Models;

public class JoinChannelModel
{
    [JsonProperty("event")]
    private string Event = "phx_join";

    [JsonProperty("payload")]
    private JoinChannelPayload Payload = new();

    [JsonProperty("ref")]
    private int Ref;

    [JsonProperty("topic")]
    private string topic = null!;

    [JsonIgnore]
    public string Id
    {
        set => topic = "hr:" + value;
    }
}

public class JoinChannelPayload
{
}
