// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

#pragma warning disable CS8618

namespace VRCOSC.Modules.Heartrate.Pulsoid.Models;

public sealed class PulsoidResponse
{
    [JsonProperty("measured_at")]
    public long MeasuredAt { get; set; }

    [JsonProperty("data")]
    public PulsoidData Data { get; set; }
}

public sealed class PulsoidData
{
    [JsonProperty("heart_rate")]
    public int HeartRate { get; set; }
}
