// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.Weather;

public class WeatherProvider
{
    private const string condition_url = "https://www.weatherapi.com/docs/weather_conditions.json";

    private readonly HttpClient httpClient = new();
    private readonly string apiKey;

    public WeatherProvider(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<Weather?> RetrieveFor(string postcode)
    {
        var uri = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q={postcode}";
        var data = await httpClient.GetAsync(new Uri(uri));
        var responseString = await data.Content.ReadAsStringAsync();
        var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(responseString)?.Current;

        if (weatherResponse is null) return null;

        using var client = new HttpClient();
        var response = await client.GetAsync(condition_url);
        var conditionContent = await response.Content.ReadAsStringAsync();
        weatherResponse.ConditionString = JsonConvert.DeserializeObject<List<WeatherCondition>>(conditionContent)?.Single(condition => condition.Code == weatherResponse.Condition.Code).Day ?? string.Empty;

        return weatherResponse;
    }
}
