// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Logging;

namespace VRCOSC.Game.Util;

public static class CurrentWorldExtractor
{
    private const string blacklist_url = "https://github.com/cyberkitsune/chatbox-club-blacklist/raw/master/npblacklist.json";
    private static readonly string logfile_location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "VRChat", "VRChat");
    private const string logfile_pattern = "output_log_*";
    private static readonly Regex world_regex = new("Fetching world information for (wrld_.*)");

    private static Blacklist? blacklist;
    private static IEnumerable<string>? ids;

    public static bool IsCurrentWorldBlacklisted { get; private set; }
    public static string? CurrentWorldId { get; private set; }

    public static async void UpdateCurrentWorld()
    {
        await requestBlacklist();

        if (blacklist is null || ids is null)
        {
            IsCurrentWorldBlacklisted = false;
            return;
        }

        var newCurrentWorldId = await getCurrentWorldId();
        if (newCurrentWorldId == CurrentWorldId) return;

        CurrentWorldId = newCurrentWorldId;
        Logger.Log($"World change detected: {CurrentWorldId}");

        if (CurrentWorldId is null)
        {
            IsCurrentWorldBlacklisted = false;
            return;
        }

        IsCurrentWorldBlacklisted = ids.Contains(CurrentWorldId);
    }

    private static async Task requestBlacklist()
    {
        try
        {
            if (blacklist is not null) return;

            using var client = new HttpClient();

            var response = await client.GetAsync(blacklist_url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            blacklist = JsonConvert.DeserializeObject<Blacklist>(content);
            if (blacklist is null) return;

            ids = blacklist.Worlds.Select(world => world.ID);
        }
        catch (Exception)
        {
            blacklist = null;
            ids = null;
        }
    }

    private static async Task<string?> getCurrentWorldId()
    {
        try
        {
            var logFile = Directory.GetFiles(logfile_location, logfile_pattern).MaxBy(d => new FileInfo(d).CreationTime);
            if (logFile is null) return null;

            using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var textReader = new StreamReader(fileStream);
            var content = await textReader.ReadToEndAsync();

            var latestWorld = world_regex.Matches(content).LastOrDefault()?.Groups.Values.LastOrDefault();
            if (latestWorld is null) return null;

            var latestWorldId = latestWorld.Captures.FirstOrDefault();
            if (latestWorldId is null) return null;

            return latestWorldId.Value;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public class Blacklist
{
    [JsonProperty("worlds")]
    public List<BlacklistedWorld> Worlds = new();
}

public class BlacklistedWorld
{
    [JsonProperty("id")]
    public string ID = null!;
}
