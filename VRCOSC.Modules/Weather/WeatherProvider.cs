// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.Weather;

public class WeatherProvider
{
    private const string api_base_url = @"https://api.weatherapi.com/v1/";
    private const string current_url_format = api_base_url + "current.json?key={0}&q={1}";
    private const string astronomy_url_format = api_base_url + "astronomy.json?key={0}&q={1}&dt={2}";

    private const string condition_url = @"https://www.weatherapi.com/docs/weather_conditions.json";

    private readonly HttpClient httpClient = new();
    private readonly string apiKey;

    public WeatherProvider(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<Weather?> RetrieveFor(string location)
    {
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

        var conditionResponseData = await httpClient.GetAsync(condition_url);
        var conditionResponseString = await conditionResponseData.Content.ReadAsStringAsync();
        var conditionResponse = JsonConvert.DeserializeObject<List<WeatherCondition>>(conditionResponseString)?.Single(condition => condition.Code == 1000);

        var dateTimeNow = DateTime.Now;

        if (dateTimeNow >= sunriseParsed && dateTimeNow < sunsetParsed)
            currentResponse.ConditionString = conditionResponse?.Day ?? string.Empty;
        else
            currentResponse.ConditionString = conditionResponse?.Night ?? string.Empty;

        return currentResponse;
    }
}
