// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.Weather;

public class WeatherProvider
{
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
        return JsonConvert.DeserializeObject<WeatherResponse>(responseString)?.Current;
    }
}
