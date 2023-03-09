// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Logging;

namespace VRCOSC.Game.Modules.Avatar;

public static class AvatarConfigLoader
{
    private static readonly string vr_chat_osc_folder_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), @"VRChat", @"VRChat", @"OSC");

    public static AvatarConfig? LoadConfigFor(string avatarId)
    {
        Logger.Log($"Attempting to load avatar {avatarId}...");

        if (!Directory.Exists(vr_chat_osc_folder_path))
        {
            Logger.Log("OSC folder unavailable");
            return null;
        }

        Logger.Log("OSC folder exists...");
        var oscFolderContents = Directory.GetDirectories(vr_chat_osc_folder_path);

        if (!oscFolderContents.Any())
        {
            Logger.Log("User folder unavailable");
            return null;
        }

        Logger.Log("User folder exists...");
        var userFolder = oscFolderContents.First();

        var avatarFolderPath = Path.Combine(userFolder, "Avatars");

        if (!Directory.Exists(avatarFolderPath))
        {
            Logger.Log("Avatars folder unavailable");
            return null;
        }

        Logger.Log("Avatars folder exists...");
        var avatarFiles = Directory.GetFiles(avatarFolderPath);

        if (!avatarFiles.Any())
        {
            Logger.Log("No configs present");
            return null;
        }

        Logger.Log($"Found {avatarFiles.Length} total configs");
        var avatarIdFiles = avatarFiles.Where(filePath => filePath.Contains(avatarId)).ToArray();

        if (!avatarIdFiles.Any())
        {
            Logger.Log("No config available for specified Id");
            return null;
        }

        Logger.Log($"Final matching config count: {avatarIdFiles.Length}");
        var avatarFile = avatarIdFiles.First();

        Logger.Log($"Attempting to load avatar config named: {avatarFile}");
        var data = JsonConvert.DeserializeObject<AvatarConfig>(File.ReadAllText(avatarFile));

        if (data is not null) Logger.Log($"Successfully loaded config for avatar {data.Name} containing {data.Parameters.Count} parameters");
        return data;
    }
}
