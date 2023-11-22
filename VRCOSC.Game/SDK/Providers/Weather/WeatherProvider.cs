// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VRCOSC.Game.SDK.Providers.Weather;

public class WeatherProvider
{
    private const string api_base_url = "https://api.weatherapi.com/v1/";
    private const string current_url_format = api_base_url + "current.json?key={0}&q={1}";
    private const string astronomy_url_format = api_base_url + "astronomy.json?key={0}&q={1}&dt={2}";

    private const string condition_url = "https://www.weatherapi.com/docs/weather_conditions.json";

    private static readonly TimeSpan api_call_delta = TimeSpan.FromMinutes(20);

    private readonly HttpClient httpClient = new();
    private readonly string apiKey;

    private DateTimeOffset lastUpdate = DateTimeOffset.MinValue;
    private string? lastLocation;
    private CurrentWeather? weather;

    private Dictionary<int, WeatherCondition>? conditions;

    public WeatherProvider(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<CurrentWeather?> RetrieveFor(string location)
    {
        if (lastUpdate + api_call_delta > DateTimeOffset.Now && location == lastLocation) return weather;

        lastUpdate = DateTimeOffset.Now;
        lastLocation = location;

        var currentUrl = string.Format(current_url_format, apiKey, location);
        var currentResponseData = await httpClient.GetAsync(new Uri(currentUrl));
        var currentResponseString = await currentResponseData.Content.ReadAsStringAsync();
        var currentResponse = JsonConvert.DeserializeObject<WeatherCurrentResponse>(currentResponseString)?.Current;

        if (currentResponse is null) return null;

        var astronomyUrl = string.Format(astronomy_url_format, apiKey, location, DateTime.Now.ToString("yyyy-MM-dd"));
        var astronomyResponseData = await httpClient.GetAsync(new Uri(astronomyUrl));
        var astronomyResponseString = await astronomyResponseData.Content.ReadAsStringAsync();
        var astronomyResponse = JsonConvert.DeserializeObject<WeatherAstronomyResponse>(astronomyResponseString)?.Astronomy.Astro;

        if (astronomyResponse is null) return null;
        if (!DateTime.TryParse(astronomyResponse.Sunrise, out var sunriseParsed)) return null;
        if (!DateTime.TryParse(astronomyResponse.Sunset, out var sunsetParsed)) return null;

        if (conditions is null) await retrieveConditions();

        var conditionResponse = conditions?[currentResponse.Condition.Code];
        var dateTimeNow = DateTime.Now;

        if (dateTimeNow >= sunriseParsed && dateTimeNow < sunsetParsed)
            currentResponse.ConditionString = conditionResponse?.Day ?? string.Empty;
        else
            currentResponse.ConditionString = conditionResponse?.Night ?? string.Empty;

        weather = currentResponse;
        return weather;
    }

    private async Task retrieveConditions()
    {
        var conditionResponseData = await httpClient.GetAsync(condition_url);
        var conditionResponseString = await conditionResponseData.Content.ReadAsStringAsync();
        var conditionData = JsonConvert.DeserializeObject<List<WeatherCondition>>(conditionResponseString);

        if (conditionData is null) return;

        conditions = new Dictionary<int, WeatherCondition>();
        conditionData.ForEach(condition => conditions.Add(condition.Code, condition));
    }
}
