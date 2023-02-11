// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Avatar;

public static class AvatarConfigLoader
{
    private static readonly string vr_chat_osc_folder_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), @"VRChat", @"VRChat", @"OSC");

    public static AvatarConfig? LoadConfigFor(string avatarId)
    {
        var oscFolder = Directory.GetDirectories(Directory.GetDirectories(vr_chat_osc_folder_path).First()).First();
        var avatarFile = Directory.GetFiles(oscFolder).First(filePath => filePath.Contains(avatarId));
        var avatarConfigRaw = File.ReadAllText(avatarFile);
        return JsonConvert.DeserializeObject<AvatarConfig>(avatarConfigRaw);
    }
}
