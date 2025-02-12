// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using Newtonsoft.Json;

namespace VRCOSC.App.Dolly;

[JsonObject(MemberSerialization.OptIn)]
public class DollySection
{
    [JsonProperty("Index")]
    public int Index { get; set; }

    [JsonProperty("PathIndex")]
    public int PathIndex { get; set; }

    [JsonProperty("FocalDistance")]
    public float FocalDistance { get; set; }

    [JsonProperty("Aperture")]
    public float Aperture { get; set; }

    [JsonProperty("Hue")]
    public float Hue { get; set; }

    [JsonProperty("Saturation")]
    public float Saturation { get; set; }

    [JsonProperty("Lightness")]
    public float Lightness { get; set; }

    [JsonProperty("LookAtMeXOffset")]
    public float LookAtMeXOffset { get; set; }

    [JsonProperty("LookAtMeYOffset")]
    public float LookAtMeYOffset { get; set; }

    [JsonProperty("Zoom")]
    public float Zoom { get; set; }

    [JsonProperty("Speed")]
    public float Speed { get; set; }

    [JsonProperty("Duration")]
    public float Duration { get; set; }

    [JsonProperty("Position")]
    public Vector3 Position { get; set; }

    [JsonProperty("Rotation")]
    public Vector3 Rotation { get; set; }

    [JsonProperty("IsLocal")]
    public bool IsLocal { get; set; }
}