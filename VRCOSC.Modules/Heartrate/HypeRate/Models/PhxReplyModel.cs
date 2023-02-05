// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.Heartrate.HypeRate.Models;

public sealed class PhxReplyModel
{
    [JsonProperty("payload")]
    public PhxReplyPayload Payload = null!;
}

public sealed class PhxReplyPayload
{
    [JsonProperty("status")]
    public string Status = null!;
}
