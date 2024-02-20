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

    public static async void UpdateCurrentWorld()
    {
        await requestBlacklist();

        if (blacklist is null || ids is null)
        {
            IsCurrentWorldBlacklisted = false;
            return;
        }

        var currentWorldId = await getCurrentWorldId();

        if (currentWorldId is null)
        {
            IsCurrentWorldBlacklisted = false;
            return;
        }

        IsCurrentWorldBlacklisted = ids.Contains(currentWorldId);
    }

    private static async Task requestBlacklist()
    {
        try
        {
            if (blacklist is not null) return;

            Logger.Log("Updating world blacklist");

            using var client = new HttpClient();

            var response = await client.GetAsync(blacklist_url);
            response.EnsureSuccessStatusCode();

            Logger.Log("Found blacklist file");

            var content = await response.Content.ReadAsStringAsync();
            blacklist = JsonConvert.DeserializeObject<Blacklist>(content);
            if (blacklist is null) return;

            Logger.Log("Successfully extracted blacklist data");

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
            Logger.Log($"Attempting to find current world using log pattern {logfile_location}\\{logfile_pattern}");

            var logFile = Directory.GetFiles(logfile_location, logfile_pattern).MaxBy(d => new FileInfo(d).CreationTime);
            if (logFile is null) return null;

            Logger.Log($"Found latest log file at {logFile}");

            using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var textReader = new StreamReader(fileStream);
            var content = await textReader.ReadToEndAsync();

            Logger.Log("Successfully read latest log file");

            var latestWorld = world_regex.Matches(content).LastOrDefault()?.Groups.Values.LastOrDefault();
            if (latestWorld is null) return null;

            var latestWorldId = latestWorld.Captures.FirstOrDefault();
            if (latestWorldId is null) return null;

            Logger.Log($"Found current world: {latestWorldId.Value}");
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
