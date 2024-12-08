// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Utils;

internal static class ChatBoxWorldBlacklist
{
    private const string blacklist_url = "https://github.com/cyberkitsune/chatbox-club-blacklist/raw/master/npblacklist.json";

    private static Blacklist? blacklist;

    public static bool IsCurrentWorldBlacklisted { get; private set; }

    internal static void Init()
    {
        VRChatLogReader.OnWorldEnter += updateCurrentWorld;
    }

    private static async void updateCurrentWorld(string worldID)
    {
        await requestBlacklist();

        IsCurrentWorldBlacklisted = blacklist?.Worlds.Any(world => world.ID == worldID) ?? false;
        Logger.Log($"Is world blacklisted?: {(IsCurrentWorldBlacklisted ? "Yes" : "No")}");
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
        }
        catch (Exception)
        {
            blacklist = null;
        }
    }
}

internal class Blacklist
{
    [JsonProperty("worlds")]
    public List<BlacklistedWorld> Worlds = new();
}

internal class BlacklistedWorld
{
    [JsonProperty("id")]
    public string ID = null!;
}
