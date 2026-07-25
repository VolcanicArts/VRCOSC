// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Providers.PiShock;

internal record struct PiShockCredentials
{
    public string Username { get; }
    public string ApiKey { get; }

    public PiShockCredentials(string username, string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        Username = username;
        ApiKey = apiKey;
    }
}

internal static class PiShockRequestFactory
{
    private static readonly Uri api_uri = new("https://api.pishock.com/");
    private static readonly HttpClient http_client = new();

    private static HttpRequestMessage constructRequest(HttpMethod method, string endpoint, PiShockCredentials credentials, HttpContent? content = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        var request = new HttpRequestMessage(method, new Uri(api_uri, endpoint));
        request.Headers.Add("X-PiShock-Username", credentials.Username);
        request.Headers.Add("X-PiShock-Api-Key", credentials.ApiKey);
        request.Content = content;
        return request;
    }

    /// <summary>
    /// Requests the account info for the provided <paramref name="credentials"/>
    /// </summary>
    public static async Task<Result<PiShockUser>> GetUser(PiShockCredentials credentials, CancellationToken token = default)
    {
        using var request = constructRequest(HttpMethod.Get, "Account", credentials);
        using var response = await http_client.SendAsync(request, token);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get user. {await response.ToVerboseMessage()}");

        var content = await response.Content.ReadAsStringAsync(token);

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get user. Response content was empty");

        var userResult = JsonSerializerSafe.TryDeserialize<PiShockUser>(content);
        return userResult.IsSuccess ? userResult : new Exception("Failed to get user. Response contained invalid JSON", userResult.Exception);
    }

    /// <summary>
    /// Gets all hubs associated with the provided <paramref name="credentials"/>
    /// </summary>
    public static async Task<Result<PiShockHub[]>> GetHubs(PiShockCredentials credentials, CancellationToken token = default)
    {
        using var request = constructRequest(HttpMethod.Get, "Hub", credentials);
        using var response = await http_client.SendAsync(request, token);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get hubs. {await response.ToVerboseMessage()}");

        var content = await response.Content.ReadAsStringAsync(token);

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get hubs. Response content was empty");

        var hubsResult = JsonSerializerSafe.TryDeserialize<PiShockHub[]>(content);
        return hubsResult.IsSuccess ? hubsResult : new Exception("Failed to get hubs. Response contained invalid JSON", hubsResult.Exception);
    }

    /// <summary>
    /// Gets all the shared shockers associated with the provided <paramref name="credentials"/>
    /// </summary>
    public static async Task<Result<PiShockShocker[]>> GetSharedShockers(PiShockCredentials credentials, CancellationToken token = default)
    {
        using var request = constructRequest(HttpMethod.Get, "Share/GetShared", credentials);
        using var response = await http_client.SendAsync(request, token);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to get shared shockers. {await response.ToVerboseMessage()}");

        if (response.StatusCode == HttpStatusCode.NoContent)
            return Array.Empty<PiShockShocker>();

        var content = await response.Content.ReadAsStringAsync(token);

        if (string.IsNullOrWhiteSpace(content))
            return new Exception("Failed to get shared shockers. Response content was empty");

        var shockersResult = JsonSerializerSafe.TryDeserialize<PiShockShocker[]>(content);
        return shockersResult.IsSuccess ? shockersResult : new Exception("Failed to get shared shockers. Response contained invalid JSON", shockersResult.Exception);
    }

    /// <summary>
    /// Claims all the <paramref name="sharecodes"/> for the provided <paramref name="credentials"/>
    /// </summary>
    public static async Task<Result> ClaimSharecodes(PiShockCredentials credentials, IReadOnlyCollection<string> sharecodes, CancellationToken token = default)
    {
        if (sharecodes is null || sharecodes.Count == 0)
            throw new ArgumentException("At least one sharecode must be provided", nameof(sharecodes));

        using var request = constructRequest(HttpMethod.Put, "Share", credentials, JsonContent.Create(new { Shares = sharecodes }));
        using var response = await http_client.SendAsync(request, token);

        if (!response.IsSuccessStatusCode)
            return new Exception($"Failed to claim sharecodes. {await response.ToVerboseMessage()}");

        return true;
    }
}