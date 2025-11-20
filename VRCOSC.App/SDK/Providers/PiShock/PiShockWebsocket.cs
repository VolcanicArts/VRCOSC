// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.Json.Serialization;

namespace VRCOSC.App.SDK.Providers.PiShock;

internal record PiShockPublishOperationResponse
{
    [JsonPropertyName("ErrorCode")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("IsError")]
    public bool IsError { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }
}

internal record PiShockPublishOperation
{
    [JsonPropertyName("Operation")]
    public string Operation { get; set; } = "PUBLISH";

    [JsonPropertyName("PublishCommands")]
    public PiShockPublishCommand[] Commands { get; set; } = [];
}

internal record PiShockPublishCommand
{
    [JsonPropertyName("Target")]
    public required string Target { get; set; }

    [JsonPropertyName("Body")]
    public required PiShockPublishCommandBody Body { get; set; }

    [JsonPropertyName("Ping")]
    public bool Ping { get; set; }
}

internal record PiShockPublishCommandBody
{
    [JsonPropertyName("id")]
    public required int ShockerId { get; set; }

    [JsonPropertyName("m")]
    public required string Mode { get; set; }

    [JsonPropertyName("i")]
    public required int Intensity { get; set; }

    [JsonPropertyName("d")]
    public required int Duration { get; set; }

    [JsonPropertyName("r")]
    public required bool Repeating { get; set; }

    [JsonPropertyName("l")]
    public required PiShockPublishCommandLogData LogData { get; set; }
}

internal record PiShockPublishCommandLogData
{
    [JsonPropertyName("u")]
    public required int User { get; set; }

    [JsonPropertyName("ty")]
    public required string Type { get; set; }

    [JsonPropertyName("w")]
    public required bool Warning { get; set; }

    [JsonPropertyName("h")]
    public required bool Hold { get; set; }

    [JsonPropertyName("o")]
    public required string Origin { get; set; }
}