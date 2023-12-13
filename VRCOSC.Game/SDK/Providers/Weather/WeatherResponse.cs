// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.SDK.Providers.Weather;

public class WeatherCurrentResponse
{
    [JsonProperty("current")]
    public CurrentWeather Current = null!;
}

public class WeatherAstronomyResponse
{
    [JsonProperty("astronomy")]
    public Astronomy Astronomy = null!;
}

public class CurrentWeather
{
    [JsonProperty("temp_c")]
    public float TempC;

    [JsonProperty("temp_f")]
    public float TempF;

    [JsonProperty("humidity")]
    public int Humidity;

    [JsonProperty("condition")]
    public Condition Condition = null!;

    [JsonIgnore]
    public string ConditionString = null!;
}

public class Astronomy
{
    [JsonProperty("astro")]
    public Astro Astro = null!;
}

public class Astro
{
    [JsonProperty("sunrise")]
    public string Sunrise = null!;

    [JsonProperty("sunset")]
    public string Sunset = null!;
}

public class Condition
{
    [JsonProperty("code")]
    public int Code;
}

public class WeatherCondition
{
    [JsonProperty("code")]
    public int Code;

    [JsonProperty("day")]
    public string Day = null!;

    [JsonProperty("night")]
    public string Night = null!;

    [JsonProperty("icon")]
    public int Icon;
}
