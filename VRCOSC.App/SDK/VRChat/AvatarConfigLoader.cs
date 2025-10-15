// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat;

public static class AvatarConfigLoader
{
    private static readonly string vr_chat_osc_folder_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "VRChat", "VRChat", "OSC");

    public static AvatarConfig? LoadConfigFor(string avatarId)
    {
        Logger.Log($"Attempting to load avatar {avatarId}...");

        if (!Directory.Exists(vr_chat_osc_folder_path))
        {
            Logger.Log("OSC folder unavailable");
            return null;
        }

        var userFolders = Directory.GetDirectories(vr_chat_osc_folder_path);

        if (userFolders.Length == 0)
        {
            Logger.Log("OSC folder has no contents");
            return null;
        }

        var userFolder = userFolders.OrderByDescending(getLatestWriteTime).First();

        if (userFolder is null)
        {
            Logger.Log("User folder unavailable");
            return null;
        }

        var avatarFolderPath = Path.Combine(userFolder, "Avatars");

        if (!Directory.Exists(avatarFolderPath))
        {
            Logger.Log("Avatars folder unavailable");
            return null;
        }

        var avatarFiles = Directory.GetFiles(avatarFolderPath);

        if (avatarFiles.Length == 0)
        {
            Logger.Log("Avatars folder has no contents");
            return null;
        }

        var avatarIdFiles = avatarFiles.Where(filePath => filePath.Contains(avatarId)).ToArray();

        if (avatarIdFiles.Length == 0)
        {
            Logger.Log("No config available for specified Id");
            return null;
        }

        var avatarFile = avatarIdFiles.First();

        var data = JsonConvert.DeserializeObject<AvatarConfig>(File.ReadAllText(avatarFile));

        if (data is not null) Logger.Log($"Successfully loaded config for avatar {data.Name} containing {data.Parameters.Count} parameters");
        return data;
    }

    /// <summary>
    /// Gets the latest write time of <paramref name="directory"/>, including subdirectories and files
    /// </summary>
    private static DateTime getLatestWriteTime(string directory)
    {
        var dir = new DirectoryInfo(directory);
        var latest = dir.LastWriteTimeUtc;

        foreach (var subDir in dir.EnumerateDirectories("*", SearchOption.AllDirectories))
        {
            if (subDir.LastWriteTimeUtc > latest)
                latest = subDir.LastWriteTimeUtc;
        }

        foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            if (file.LastWriteTimeUtc > latest)
                latest = file.LastWriteTimeUtc;
        }

        return latest;
    }
}