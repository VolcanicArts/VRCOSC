// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.Heartrate.HypeRate.Models;

public sealed class EventModel
{
    [JsonProperty("event")]
    public string Event = null!;
}
