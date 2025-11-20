// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json.Serialization;

namespace VRCOSC.App.SDK.Providers.PiShock;

internal abstract record LegacyPiShockBaseRequest
{
    [JsonPropertyName("Username")]
    public required string Username { get; set; }

    [JsonPropertyName("Code")]
    public required string ShareCode { get; set; }

    [JsonPropertyName("Apikey")]
    public required string ApiKey { get; set; }
}

internal abstract record LegacyPiShockBaseActionRequest : LegacyPiShockBaseRequest
{
    [JsonPropertyName("Name")]
    public required string AppName { get; set; }

    [JsonPropertyName("Op")]
    public string Op => ((int)Mode).ToString();

    [JsonPropertyName("Duration")]
    public required string Duration { get; set; }

    [JsonIgnore]
    protected virtual PiShockMode Mode => default;
}

internal record LegacyPiShockShockerInfoRequest : LegacyPiShockBaseRequest
{
}

internal record LegacyPiShockVibrateActionRequest : LegacyPiShockBaseActionRequest
{
    protected override PiShockMode Mode => PiShockMode.Vibrate;

    [JsonPropertyName("Intensity")]
    public required string Intensity { get; set; }
}

internal record LegacyPiShockShocker
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("shockerId")]
    public int Id { get; set; }

    [JsonPropertyName("isPaused")]
    public bool IsPaused { get; set; }
}