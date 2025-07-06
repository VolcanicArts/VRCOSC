// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global

namespace VRCOSC.App.SDK.Providers.PiShock;

internal record PiShockSerialCommand
{
    [JsonPropertyName("cmd")]
    public required string Command { get; set; }

    [JsonPropertyName("value")]
    public PiShockSerialBody? Body { get; set; }
}

internal record PiShockSerialBody
{
    [JsonPropertyName("id")]
    public required int ShockerId { get; set; }

    [JsonPropertyName("op")]
    public required string Op { get; set; }

    [JsonPropertyName("duration")]
    public required int Duration { get; set; }

    [JsonPropertyName("intensity")]
    public required int Intensity { get; set; }
}

internal record PiShockSerialTerminalInfoResponse
{
    [JsonPropertyName("shockers")]
    public List<PiShockSerialTerminalInfoShocker> Shockers { get; set; } = [];
}

internal record PiShockSerialTerminalInfoShocker
{
    [JsonPropertyName("id")]
    public int ShockerId { get; set; }
}