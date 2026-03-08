// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO.Ports;
using System.Text.Json.Serialization;

namespace VRCOSC.App.SDK.Providers.PiShock;

internal class PiShockUser
{
    [JsonPropertyName("UserId")]
    public int UserId { get; set; }
}

internal class PiShockClient
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }
}

internal class PiShockShocker
{
    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }

    [JsonPropertyName("shockerId")]
    public int ShockerId { get; set; }

    [JsonPropertyName("shareCode")]
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