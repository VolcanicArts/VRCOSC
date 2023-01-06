// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.Weather;

public class WeatherResponse
{
    [JsonProperty("current")]
    public Weather Current = null!;
}

public class Weather
{
    [JsonProperty("temp_c")]
    public float TempC;

    [JsonProperty("temp_f")]
    public float TempF;

    [JsonProperty("humidity")]
    public int Humidity;

    [JsonProperty("condition")]
    public Condition Condition = null!;
}

public class Condition
{
    [JsonProperty("code")]
    public int Code;
}
