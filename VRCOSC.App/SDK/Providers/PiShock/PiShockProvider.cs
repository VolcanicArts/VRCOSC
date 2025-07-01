// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.PiShock;

public class PiShockProvider : IDisposable
{
    private const string auth_endpoint = "https://auth.pishock.com/Auth";
    private const string api_endpoint = "https://ps.pishock.com/PiShock";
    private const string broker_endpoint = "wss://broker.pishock.com/v2";

    private readonly HttpClient httpClient = new();
    private WebSocketClient webSocket = null!;
    private readonly string username;
    private readonly string apiKey;

    private int userId = -1;

    // client (hub) IDs - [sharecodes]
    private Dictionary<int, List<PiShockSharedShocker>> hubSharecodes { get; } = [];
    private int ownedHubId = -1;

    public PiShockProvider(string username, string apiKey)
    {
        this.username = username ?? throw new ArgumentNullException(nameof(username));
        this.apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    private async Task authenticate()
    {
        var response = await httpClient.GetAsync($"{auth_endpoint}/GetUserIfAPIKeyValid?apikey={apiKey}&username={username}");
        response.EnsureSuccessStatusCode();

        userId = JsonConvert.DeserializeObject<PiShockAuthenticationResponse>(await response.Content.ReadAsStringAsync())?.UserId ?? -1;
    }

    public async Task GetOwnedShockersAsync()
    {
        try
        {
            var requestUri = $"{api_endpoint}/GetUserDevices?UserId={userId}&Token={apiKey}&api=true";
            var response = await httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var devices = JsonConvert.DeserializeObject<List<PiShockHub>>(content);
            ownedHubId = devices?.FirstOrDefault()?.ClientId ?? -1;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(PiShockProvider)} has experienced an error when receiving shockers");
        }
    }

    public async Task GetSharedShockersAsync()
    {
        hubSharecodes.Clear();

        try
        {
            var requestUri = $"{api_endpoint}/GetShareCodesByOwner?UserId={userId}&Token={apiKey}&api=true";
            var response = await httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // owner's username - shockers
            var devices = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(content)!;
            var sharedShockers = await getShockerByShareId(devices.Values.SelectMany(t => t));

            foreach (var pair in sharedShockers)
            {
                var userSharedShockers = pair.Value;

                foreach (var sharedShocker in userSharedShockers)
                {
                    if (hubSharecodes.TryGetValue(sharedShocker.ClientId, out var sharecodeList))
                        sharecodeList.Add(sharedShocker);
                    else
                        hubSharecodes[sharedShocker.ClientId] = new List<PiShockSharedShocker> { sharedShocker };
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(PiShockProvider)} has experienced an error when receiving shockers");
        }
    }

    private async Task<Dictionary<string, List<PiShockSharedShocker>>> getShockerByShareId(IEnumerable<int> shareIds)
    {
        var requestUri = string.Join("&", new[] { $"{api_endpoint}/GetShockersByShareIds?UserId={userId}&Token={apiKey}&api=true" }.Concat(shareIds.Select(shareId => $"shareIds={shareId}")));
        var response = await httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var devices = JsonConvert.DeserializeObject<Dictionary<string, List<PiShockSharedShocker>>>(content);
        return devices!;
    }

    public async Task<bool> Initialise()
    {
        await authenticate();
        if (userId == -1) return false;

        await GetOwnedShockersAsync();
        await GetSharedShockersAsync();

        webSocket = new WebSocketClient($"{broker_endpoint}?Username={username}&ApiKey={apiKey}", 2000, 3);

        await webSocket.ConnectAsync();

        return true;
    }

    public async Task Teardown()
    {
        await webSocket.DisconnectAsync();
        userId = -1;
    }

    public async Task<PiShockResult> ExecuteAsync(int shockerId, PiShockMode mode, int intensity, int duration)
    {
        if (userId == -1) return new PiShockResult(false, "Not initialised");

        var clientId = ownedHubId;
        var channel = $"c{clientId}-ops";

        await executeAsync(channel, [shockerId], mode, intensity, duration);
        return new PiShockResult(true, string.Empty);
    }

    public async Task<PiShockResult> ExecuteAsync(IEnumerable<string> shareCodes, PiShockMode mode, int intensity, int duration)
    {
        if (userId == -1) return new PiShockResult(false, "Not initialised");

        var shareCodeList = shareCodes.ToList();
        var missingShareCodes = new List<string>();

        foreach (var code in shareCodeList)
        {
            var exists = hubSharecodes.Any(pair => pair.Value.Any(sharedShocker => sharedShocker.ShareCode == code));

            if (!exists)
                missingShareCodes.Add(code);
        }

        foreach (var code in missingShareCodes)
        {
            var claimed = await claimShareCode(code);

            if (!claimed.Success)
                return claimed;
        }

        if (missingShareCodes.Count != 0)
            await GetSharedShockersAsync();

        foreach (var code in shareCodeList)
        {
            var exists = hubSharecodes.Any(pair => pair.Value.Any(sharedShocker => sharedShocker.ShareCode == code));

            if (!exists)
                return new PiShockResult(false, $"Sharecode {code} does not exist");
        }

        var sharedShockers = new List<PiShockSharedShocker>();

        foreach (var code in shareCodeList)
        {
            var shocker = hubSharecodes
                          .SelectMany(pair => pair.Value)
                          .FirstOrDefault(s => s.ShareCode == code);

            if (shocker != null)
                sharedShockers.Add(shocker);
            else
                Logger.Log($"Shocker doesn't exist for share code {code}");
        }

        var channels = sharedShockers.Select(shocker => $"c{shocker.ClientId}-sops-{shocker.ShareCode}").ToList();
        var shockerIds = sharedShockers.Select(shocker => shocker.ShockerId).ToList();

        foreach (var channel in channels)
        {
            await executeAsync(channel, shockerIds, mode, intensity, duration);
        }

        return new PiShockResult(true, string.Empty);
    }

    private async Task executeAsync(string channel, List<int> shockerIds, PiShockMode mode, int intensity, int duration)
    {
        var content = JsonConvert.SerializeObject(new PiShockPublishOperation
        {
            Commands = shockerIds.Select(shockerId => new PiShockPublishCommand
            {
                Target = channel,
                Body = new PiShockPublishCommandBody
                {
                    ShockerId = shockerId,
                    Mode = modeToCode(mode),
                    Intensity = intensity,
                    Duration = duration,
                    Repeating = false,
                    LogData = new PiShockPublishCommandLogData
                    {
                        User = userId,
                        Type = "api",
                        Warning = false,
                        Hold = false,
                        Origin = $"{AppManager.APP_NAME}-{username}"
                    }
                }
            }).ToList()
        });

        await webSocket.SendAsync(content);
    }

    private async Task<PiShockResult> claimShareCode(string shareCode)
    {
        try
        {
            var shocker = await legacyRetrieveShockerInfo(shareCode);
            if (shocker is null) return new PiShockResult(false, $"Shocker for sharecode {shareCode} does not exist");

            var request = getLegacyRequestForMode(PiShockMode.Vibrate, 1, 1);
            request.AppName = AppManager.APP_NAME;
            request.Username = username;
            request.APIKey = apiKey;
            request.ShareCode = shareCode;

            var response = await httpClient.PostAsync("https://ps.pishock.com/pishock/operate", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            return new PiShockResult(responseString.Contains("Succeeded") || responseString.Contains("Attempted"), responseString);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(PiShockProvider)} has experienced an exception");
            return new PiShockResult(false, "Exception occurred");
        }
    }

    private async Task<PiShockShocker?> legacyRetrieveShockerInfo(string shareCode)
    {
        try
        {
            var request = new ShockerInfoPiShockRequest
            {
                Username = username,
                APIKey = apiKey,
                ShareCode = shareCode
            };

            var response = await httpClient.PostAsync("https://do.pishock.com/api/GetShockerInfo", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PiShockShocker>(responseString);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"{nameof(PiShockProvider)} has experienced an exception");
            return null;
        }
    }

    private static ActionPiShockRequest getLegacyRequestForMode(PiShockMode mode, int duration, int intensity) => mode switch
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        httpClient.Dispose();
        webSocket.Dispose();
    }

    private string modeToCode(PiShockMode mode)
    {
        switch (mode)
        {
            case PiShockMode.Shock:
                return "s";

            case PiShockMode.Vibrate:
                return "v";

            case PiShockMode.Beep:
                return "b";

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public record PiShockResult(bool Success, string Message);

public enum PiShockMode
{
    Shock,
    Vibrate,
    Beep
}

[JsonObject(MemberSerialization.OptIn)]
public class PiShockAuthenticationResponse
{
    [JsonProperty("UserId")]
    public int UserId { get; set; }
}

[JsonObject(MemberSerialization.OptIn)]
public class PiShockHub
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("clientId")]
    public int ClientId { get; set; }

    [JsonProperty("shockers")]
    public List<PiShockShocker> Shockers { get; set; } = [];
}

[JsonObject(MemberSerialization.OptIn)]
public class PiShockShocker
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("shockerId")]
    public int Id { get; set; }

    [JsonProperty("isPaused")]
    public bool IsPaused { get; set; }
}

[JsonObject(MemberSerialization.OptIn)]
public class PiShockSharedShocker
{
    [JsonProperty("shockerName")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("clientId")]
    public int ClientId { get; set; }

    [JsonProperty("shockerId")]
    public int ShockerId { get; set; }

    [JsonProperty("isPaused")]
    public bool IsPaused { get; set; }

    [JsonProperty("shareCode")]
    public string ShareCode { get; set; } = string.Empty;
}