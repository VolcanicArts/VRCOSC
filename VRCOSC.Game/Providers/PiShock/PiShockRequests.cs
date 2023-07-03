// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Providers.PiShock;

public abstract class BasePiShockRequest
{
    [JsonProperty("Username")]
    public string Username = null!;

    [JsonProperty("Code")]
    public string ShareCode = null!;

    [JsonProperty("Apikey")]
    public string APIKey = null!;
}

public abstract class ActionPiShocKRequest : BasePiShockRequest
{
    [JsonProperty("Name")]
    public string AppName = null!;

    [JsonProperty("Op")]
    public string Op => ((int)Mode).ToString();

    [JsonProperty("Duration")]
    public string Duration = null!;

    [JsonIgnore]
    protected virtual PiShockMode Mode => default;
}

public class ShockerInfoPiShockRequest : BasePiShockRequest
{
}

public class ShockPiShockRequest : ActionPiShocKRequest
{
    protected override PiShockMode Mode => PiShockMode.Shock;

    [JsonProperty("Intensity")]
    public string Intensity = null!;
}

public class VibratePiShockRequest : ActionPiShocKRequest
{
    protected override PiShockMode Mode => PiShockMode.Vibrate;

    [JsonProperty("Intensity")]
    public string Intensity = null!;
}

public class BeepPiShockRequest : ActionPiShocKRequest
{
    protected override PiShockMode Mode => PiShockMode.Beep;
}
