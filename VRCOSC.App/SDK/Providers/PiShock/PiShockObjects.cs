// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO.Ports;
using System.Text.Json.Serialization;

namespace VRCOSC.App.SDK.Providers.PiShock;

internal class PiShockUser
{
    [JsonPropertyName("UserId")]
    public int Id { get; set; }

    [JsonPropertyName("Username")]
    public string Username { get; set; } = null!;
}

internal class PiShockHub
{
    [JsonPropertyName("HubId")]
    public int Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = null!;
}

internal class PiShockShocker
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    [JsonPropertyName("OwnerId")]
    public int OwnerId { get; set; }

    [JsonPropertyName("ClientId")]
    public int HubId { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("ShareCode")]
    public string ShareCode { get; set; } = null!;
}

internal record PiShockSerialInstance(PiShockSerialTerminalInfoResponse Info, SerialPort Serial);

public record PiShockResult(bool Success, string Message);

public enum PiShockMode
{
    Shock,
    Vibrate,
    Beep,
    End
}