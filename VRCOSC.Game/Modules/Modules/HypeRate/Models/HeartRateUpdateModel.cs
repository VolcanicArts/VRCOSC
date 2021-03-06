// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.HypeRate.Models;

public class HeartRateUpdateModel
{
    [JsonProperty("payload")]
    public HeartRateUpdatePayload Payload = null!;
}

public class HeartRateUpdatePayload
{
    [JsonProperty("hr")]
    public int HeartRate;
}
