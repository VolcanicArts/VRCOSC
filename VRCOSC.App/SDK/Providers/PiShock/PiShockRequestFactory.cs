// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.PiShock;

internal static class PiShockRequestFactory
{
    private const string api_uri = "https://api.pishock.com";
    private static readonly HttpClient http_client = new();

    private static HttpRequestMessage constructRequest(HttpMethod method, string endpoint, string username, string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        var request = new HttpRequestMessage(method, new Uri($"{api_uri}/{endpoint}"));
        request.Headers.Add("X-PiShock-Username", username);
        request.Headers.Add("X-PiShock-Api-Key", apiKey);
        return request;
    }

    /// <summary>
    /// Requests the account info for the provided <paramref name="username"/> and <paramref name="apiKey"/>
    /// </summary>
    public static async Task<Result<PiShockUser>> GetUser(string username, string apiKey)
    {
        var request = constructRequest(HttpMethod.Get, "Account", username, apiKey);
        var response = await http_client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get user. Response status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get user. Response content was empty");

        var userResult = JsonSerializerSafe.TryDeserialize<PiShockUser>(content);

        if (!userResult.IsSuccess)
            return new Exception("Failed to get user. Response content contains invalid JSON");

        return userResult.Value;
    }

    /// <summary>
    /// Gets all hubs associated with the provided <paramref name="username"/> and <paramref name="apiKey"/>
    /// </summary>
    public static async Task<Result<PiShockHub[]>> GetHubs(string username, string apiKey)
    {
        var request = constructRequest(HttpMethod.Get, "Hub", username, apiKey);
        var response = await http_client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get hubs. Response status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get hubs. Response content was empty");

        var hubsResult = JsonSerializerSafe.TryDeserialize<PiShockHub[]>(content);

        if (!hubsResult.IsSuccess)
            return new Exception("Failed to get hubs. Response content contains invalid JSON");

        var data = hubsResult.Value;

        if (data.Length == 0)
            return new Exception("Failed to get hubs. No hubs found");

        return data;
    }

    /// <summary>
    /// Gets all the shared shockers associated with the provided <paramref name="username"/> and <paramref name="apiKey"/>
    /// </summary>
    public static async Task<Result<PiShockShocker[]>> GetShockers(string username, string apiKey)
    {
        var request = constructRequest(HttpMethod.Get, "Share/GetShared", username, apiKey);
        var response = await http_client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get sharecodes by owner. Response status code: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get sharecodes by owner. Response content was empty");

        var shockersResult = JsonSerializerSafe.TryDeserialize<PiShockShocker[]>(content);

        if (!shockersResult.IsSuccess)
            return new Exception("Failed to get sharecodes by owner. Response content contains invalid JSON");

        return shockersResult.Value;
    }

    /// <summary>
    /// Claims all the <paramref name="sharecodes"/> for a provided <paramref name="username"/> and <paramref name="apiKey"/>
    /// </summary>
    public static async Task<Result> ClaimSharecodes(string username, string apiKey, string[] sharecodes)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(sharecodes.Length, 0, nameof(sharecodes));

        var request = constructRequest(HttpMethod.Put, "Share", username, apiKey);
        request.Content = JsonContent.Create(new { Shares = sharecodes });
        var response = await http_client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to claim sharecodes. Response status code: {response.StatusCode}");

        return true;
    }
}