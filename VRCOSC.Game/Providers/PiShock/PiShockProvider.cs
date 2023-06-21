// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace VRCOSC.Game.Providers.PiShock;

public class PiShockProvider
{
    private const string app_name = "VRCOSC";
    private const string api_url = "https://do.pishock.com/api/apioperate";

    private readonly HttpClient client;
    private readonly string apiKey;

    public string Username { get; set; } = string.Empty;
    public string ShareCode { get; set; } = string.Empty;

    public PiShockProvider(string apiKey)
    {
        this.apiKey = apiKey;

        client = new HttpClient
        {
            DefaultRequestHeaders = { { "Content-Type", "application/json" } }
        };
    }

    public async void Execute(PiShockMode mode, int duration, int intensity)
    {
        if (duration is < 1 or > 15) throw new InvalidOperationException($"{nameof(duration)} must be between 1 and 15");
        if (intensity is < 1 or > 100) throw new InvalidOperationException($"{nameof(intensity)} must be between 1 and 100");

        var request = getRequestForMode(mode, duration, intensity);
        fillRequestData(request);

        await client.PostAsync(api_url, new StringContent(JsonConvert.SerializeObject(request)));
    }

    private static BasePiShockRequest getRequestForMode(PiShockMode mode, int duration, int intensity) => mode switch
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

    private void fillRequestData(BasePiShockRequest request)
    {
        request.AppName = app_name;
        request.APIKey = apiKey;
        request.ShareCode = ShareCode;
        request.Username = Username;
    }
}

public enum PiShockMode
{
    Shock,
    Vibrate,
    Beep
}
