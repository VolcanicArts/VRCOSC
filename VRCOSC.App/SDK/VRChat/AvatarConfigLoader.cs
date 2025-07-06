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

        var oscFolderContents = Directory.GetDirectories(vr_chat_osc_folder_path);

        if (oscFolderContents.Length == 0)
        {
            Logger.Log("User folder unavailable");
            return null;
        }

        var userFolder = oscFolderContents.OrderByDescending(path => new DirectoryInfo(path).LastWriteTime).First();

        var avatarFolderPath = Path.Combine(userFolder, "Avatars");

        if (!Directory.Exists(avatarFolderPath))
        {
            Logger.Log("Avatars folder unavailable");
            return null;
        }

        var avatarFiles = Directory.GetFiles(avatarFolderPath);

        if (avatarFiles.Length == 0)
        {
            Logger.Log("No configs present");
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
}