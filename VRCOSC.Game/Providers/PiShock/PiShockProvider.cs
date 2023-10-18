// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VRCOSC.Game.Providers.PiShock;

public class PiShockProvider
{
    private const string app_name = "VRCOSC";
    private const string base_api_url = "https://do.pishock.com/api";
    private const string action_api_url = base_api_url + "/apioperate";
    private const string info_api_url = base_api_url + "/GetShockerInfo";

    private readonly HttpClient client = new();

    public async Task<PiShockResponse> Execute(string username, string apiKey, string sharecode, PiShockMode mode, int duration, int intensity)
    {
        if (string.IsNullOrEmpty(username)) return new PiShockResponse(false, "Invalid username");
        if (string.IsNullOrEmpty(apiKey)) return new PiShockResponse(false, "Invalid API key");

        if (duration is < 1 or > 15) return new PiShockResponse(false, "Duration must be between 1 and 15");
        if (intensity is < 1 or > 100) return new PiShockResponse(false, "Intensity must be between 1 and 100");

        var shocker = await retrieveShockerInfo(username, apiKey, sharecode);
        if (shocker is null) return new PiShockResponse(false, "Shocker does not exist");

        duration = Math.Min(duration, shocker.MaxDuration);
        intensity = Math.Min(intensity, shocker.MaxIntensity);

        var request = getRequestForMode(mode, duration, intensity);
        request.AppName = app_name;
        request.Username = username;
        request.APIKey = apiKey;
        request.ShareCode = sharecode;

        try
        {
            var response = await client.PostAsync(action_api_url, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            return new PiShockResponse(responseString is "Operation Succeeded." or "Operation Attempted.", responseString, duration, intensity);
        }
        catch (Exception e)
        {
            return new PiShockResponse(false, e.Message);
        }
    }

    private async Task<PiShockShocker?> retrieveShockerInfo(string username, string apiKey, string sharecode)
    {
        try
        {
            var request = new ShockerInfoPiShockRequest
            {
                Username = username,
                APIKey = apiKey,
                ShareCode = sharecode
            };

            var response = await client.PostAsync(info_api_url, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PiShockShocker>(responseString);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static ActionPiShockRequest getRequestForMode(PiShockMode mode, int duration, int intensity) => mode switch
    {
        PiShockMode.Shock => new ShockPiShockRequest
        {
            Duration = duration.ToString(),
            Intensity = intensity.ToString()
        },
        PiShockMode.Vibrate => new VibratePiShockRequest
        {
            Duration = duration.ToString(),
            Intensity = intensity.ToString()
        },
        PiShockMode.Beep => new BeepPiShockRequest
        {
            Duration = duration.ToString()
        },
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };
}

public record PiShockResponse(bool Success, string Message, int FinalDuration = -1, int FinalIntensity = -1);

public class PiShockShocker
{
    [JsonProperty("clientId")]
    public int ClientID;

    [JsonProperty("id")]
    public int ID;

    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("paused")]
    public bool Paused;

    [JsonProperty("maxIntensity")]
    public int MaxIntensity;

    [JsonProperty("maxDuration")]
    public int MaxDuration;

    [JsonProperty("online")]
    public bool Online;
}

public enum PiShockMode
{
    Shock,
    Vibrate,
    Beep
}
