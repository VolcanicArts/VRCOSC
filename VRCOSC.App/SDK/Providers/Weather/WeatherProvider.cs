// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.Weather;

public class WeatherProvider
{
    private const string api_base_url = "https://api.weatherapi.com/v1/";
    private const string current_url_format = api_base_url + "current.json?key={0}&q={1}";
    private const string astronomy_url_format = api_base_url + "astronomy.json?key={0}&q={1}&dt={2}";

    private const string condition_url = "https://www.weatherapi.com/docs/weather_conditions.json";

    private static readonly TimeSpan cache_expire_time = TimeSpan.FromMinutes(20);

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

    public async Task<CurrentWeather?> RetrieveFor(string location, DateTime locationDateTime)
    {
        try
        {
            if (lastUpdate + cache_expire_time > DateTimeOffset.Now && location == lastLocation) return weather;

            lastUpdate = DateTimeOffset.Now;
            lastLocation = location;

            var currentUrl = string.Format(current_url_format, apiKey, location);
            var currentResponseData = await httpClient.GetAsync(new Uri(currentUrl));

            if (!currentResponseData.IsSuccessStatusCode)
            {
                Logger.Log($"{nameof(WeatherProvider)} could not retrieve {nameof(currentResponseData)}");
                return weather;
            }

            var currentResponseString = await currentResponseData.Content.ReadAsStringAsync();
            var currentResponse = JsonConvert.DeserializeObject<WeatherCurrentResponse>(currentResponseString)?.Current;

            if (currentResponse is null)
            {
                Logger.Log($"{nameof(WeatherProvider)} could not parse {nameof(currentResponse)}");
                return weather;
            }

            var astronomyUrl = string.Format(astronomy_url_format, apiKey, location, DateTime.Now.ToString("yyyy-MM-dd"));
            var astronomyResponseData = await httpClient.GetAsync(new Uri(astronomyUrl));

            if (!astronomyResponseData.IsSuccessStatusCode)
            {
                Logger.Log($"{nameof(WeatherProvider)} could not retrieve {nameof(astronomyResponseData)}");
                return weather;
            }

            var astronomyResponseString = await astronomyResponseData.Content.ReadAsStringAsync();
            var astronomyResponse = JsonConvert.DeserializeObject<WeatherAstronomyResponse>(astronomyResponseString)?.Astronomy.Astro;

            if (astronomyResponse is null)
            {
                Logger.Log($"{nameof(WeatherProvider)} could not parse {nameof(astronomyResponse)}");
                return weather;
            }

            currentResponse.ConditionString = "Unknown";

            if (!DateTime.TryParse(astronomyResponse.Sunrise, out var sunriseParsed)) return weather;
            if (!DateTime.TryParse(astronomyResponse.Sunset, out var sunsetParsed)) return weather;

            await retrieveConditions();

            if (conditions is not null)
            {
                var conditionResponse = conditions[currentResponse.Condition.Code];
                currentResponse.ConditionString = locationDateTime >= sunriseParsed && locationDateTime < sunsetParsed ? conditionResponse.Day : conditionResponse.Night;
            }

            weather = currentResponse;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(WeatherProvider)} experienced an issue");
            weather = null;
        }

        return weather;
    }

    private async Task retrieveConditions()
    {
        try
        {
            if (conditions is not null) return;

            var conditionResponseData = await httpClient.GetAsync(condition_url);

            if (!conditionResponseData.IsSuccessStatusCode)
            {
                Logger.Log($"{nameof(WeatherProvider)} could not retrieve {nameof(conditionResponseData)}");
                return;
            }

            var conditionResponseString = await conditionResponseData.Content.ReadAsStringAsync();
            var conditionResponse = JsonConvert.DeserializeObject<List<WeatherCondition>>(conditionResponseString);

            if (conditionResponse is null)
            {
                Logger.Log($"{nameof(WeatherProvider)} could not parse {nameof(conditionResponse)}");
                return;
            }

            conditions = new Dictionary<int, WeatherCondition>();
            conditionResponse.ForEach(condition => conditions.Add(condition.Code, condition));
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(WeatherProvider)} experienced an issue");
            conditions = null;
        }
    }
}
