// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.PiShock;

internal static class PiShockRequestFactory
{
    private static readonly HttpClient http_client = new();

    private const string auth_endpoint = "https://auth.pishock.com/Auth";
    private const string api_endpoint = "https://ps.pishock.com/PiShock";
    private const string claim_endpoint = "https://api.pishock.com/Share";

    private static void validateArguments(int userId, string apiKey)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 0);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
    }

    public static async Task<Result<PiShockUser>> AuthenticateUser(string username, string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        var requestEndpoint = $"{auth_endpoint}/GetUserIfAPIKeyValid?apikey={apiKey}&username={username}";
        var requestUri = new Uri(requestEndpoint);

        var response = await http_client.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to authenticate user. Response status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to authenticate user. Response content was empty");

        var deserializeResult = JsonSerializerSafe.TryDeserialize<PiShockUser>(content);

        if (!deserializeResult.IsSuccess)
            return new Exception("Failed to authenticate user. Response content contains invalid JSON");

        var data = deserializeResult.Value;

        return data;
    }

    public static async Task<Result<PiShockClient[]>> GetUserDevices(int userId, string apiKey)
    {
        validateArguments(userId, apiKey);

        var requestEndpoint = $"{api_endpoint}/GetUserDevices?UserId={userId}&Token={apiKey}&api=true";
        var requestUri = new Uri(requestEndpoint);

        var response = await http_client.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get user devices. Response status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get user devices. Response content was empty");

        var deserializeResult = JsonSerializerSafe.TryDeserialize<PiShockClient[]>(content);

        if (!deserializeResult.IsSuccess)
            return new Exception("Failed to get user devices. Response content contains invalid JSON");

        var data = deserializeResult.Value;

        if (data.Length == 0)
            return new Exception("Failed to get user devices. No devices found");

        return data;
    }

    /// <summary>
    /// Gets all the sharecode shareIDs from a provided <paramref name="userId"/> and <paramref name="apiKey"/>
    /// </summary>
    /// <returns>A dictionary of "Owner's Username - Sharecode Share ID Array"</returns>
    public static async Task<Result<Dictionary<string, int[]>>> GetShareCodesByOwner(int userId, string apiKey)
    {
        validateArguments(userId, apiKey);

        var requestEndpoint = $"{api_endpoint}/GetShareCodesByOwner?UserId={userId}&Token={apiKey}&api=true";
        var requestUri = new Uri(requestEndpoint);

        var response = await http_client.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get sharecodes by owner. Response status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get sharecodes by owner. Response content was empty");

        var deserializeResult = JsonSerializerSafe.TryDeserialize<Dictionary<string, int[]>>(content);

        if (!deserializeResult.IsSuccess)
            return new Exception("Failed to get sharecodes by owner. Response content contains invalid JSON");

        var data = deserializeResult.Value;

        return data;
    }

    /// <summary>
    /// Gets all the shockers from a provided <paramref name="userId"/> and <paramref name="apiKey"/> and <paramref name="shareIDs"/>
    /// </summary>
    /// <returns>A dictionary of "Owner's Username - Shocker Array"</returns>
    public static async Task<Result<Dictionary<string, PiShockShocker[]>>> GetShockersByShareIDs(int userId, string apiKey, IEnumerable<int> shareIDs)
    {
        validateArguments(userId, apiKey);

        var requestEndpoint = $"{api_endpoint}/GetShockersByShareIds?UserId={userId}&Token={apiKey}&api=true&{string.Join('&', shareIDs.Select(shareId => $"shareIds={shareId}"))}";
        var requestUri = new Uri(requestEndpoint);

        var response = await http_client.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get shockers from share IDs. Response status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get shockers from share IDs. Response content was empty");

        var deserializeResult = JsonSerializerSafe.TryDeserialize<Dictionary<string, PiShockShocker[]>>(content);

        if (!deserializeResult.IsSuccess)
            return new Exception("Failed to get shockers from share IDs. Response content contains invalid JSON");

        var data = deserializeResult.Value;

        return data;
    }

    public static async Task<Result> ClaimSharecodes(string username, string apiKey, string[] sharecodes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentOutOfRangeException.ThrowIfEqual(sharecodes.Length, 0, nameof(sharecodes));

        var requestUri = new Uri(claim_endpoint);
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
        request.Headers.Add("X-PiShock-Username", username);
        request.Headers.Add("X-PiShock-Api-Key", apiKey);
        request.Content = JsonContent.Create(new { Shares = sharecodes });

        var response = await http_client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to claim sharecodes. Response status code: {response.StatusCode}");

        return true;
    }
}