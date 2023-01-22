// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.Game;

namespace VRCOSC.Modules.Weather;

public static class WeatherProvider
{
    private static readonly HttpClient http_client = new();

    public static async Task<Weather?> RetrieveFor(string postcode)
    {
        var uri = $"https://api.weatherapi.com/v1/current.json?key={VRCOSCSecrets.GetKey(VRCOSCSecrets.Keys.Weather)}&q={postcode}";
        var data = await http_client.GetAsync(new Uri(uri));
        var responseString = await data.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<WeatherResponse>(responseString)?.Current;
    }
}
