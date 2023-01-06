// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using Windows.Web.Http;
using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Modules.Weather;

public static class WeatherProvider
{
    private static readonly HttpClient http_client = new();

    public static async Task<Weather?> RetrieveFor(string postcode)
    {
        var uri = $"https://api.weatherapi.com/v1/current.json?key={VRCOSCSecrets.KEYS_WEATHER}&q={postcode}";
        var data = await http_client.TryGetAsync(new Uri(uri));
        var responseString = await data.ResponseMessage.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<WeatherResponse>(responseString)?.Current;
    }
}
