// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCOSC.App.SDK.Providers.PiShock;

[JsonObject(MemberSerialization.OptIn)]
internal record PiShockPublishOperationResponse
{
    [JsonProperty("ErrorCode")]
    public int? ErrorCode;

    [JsonProperty]
    public bool IsError;

    [JsonProperty("Message")]
    public string Message = null!;
}

[JsonObject(MemberSerialization.OptIn)]
internal record PiShockPublishOperation
{
    [JsonProperty("Operation")]
    public readonly string Operation = "PUBLISH";

    [JsonProperty("PublishCommands")]
    public List<PiShockPublishCommand> Commands = [];
}

[JsonObject(MemberSerialization.OptIn)]
internal record PiShockPublishCommand
{
    [JsonProperty("Target")]
    public required string Target;

    [JsonProperty("Body")]
    public required PiShockPublishCommandBody Body;

    [JsonProperty("Ping")]
    public bool Ping;
}

[JsonObject(MemberSerialization.OptIn)]
internal record PiShockPublishCommandBody
{
    [JsonProperty("id")]
    public required int ShockerId;

    [JsonProperty("m")]
    public required string Mode;

    [JsonProperty("i")]
    public required int Intensity;

    [JsonProperty("d")]
    public required int Duration;

    [JsonProperty("r")]
    public required bool Repeating;

    [JsonProperty("l")]
    public required PiShockPublishCommandLogData LogData;
}

[JsonObject(MemberSerialization.OptIn)]
internal record PiShockPublishCommandLogData
{
    [JsonProperty("u")]
    public required int User;

    [JsonProperty("ty")]
    public required string Type;

    [JsonProperty("w")]
    public required bool Warning;

    [JsonProperty("h")]
    public required bool Hold;

    [JsonProperty("o")]
    public required string Origin;
}