// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Newtonsoft.Json;

namespace VRCOSC.Game;

public class VRManifest
{
    [JsonProperty("source")]
    public string Source = "builtin";

    [JsonProperty("applications")]
    public VRApplication[] Applications =
    {
        new()
    };
}

public class VRApplication
{
    [JsonProperty("app_key")]
    public string AppKey = "volcanicarts.vrcosc";

    [JsonProperty("launch_type")]
    public string LaunchType = "binary";

    [JsonProperty("binary_path_windows")]
    public string BinaryPathWindows = @$"C:\Users\{Environment.UserName}\AppData\Local\VRCOSC\VRCOSC.exe";

    [JsonProperty("is_dashboard_overlay")]
    public bool IsDashboardOverlay = true;

    [JsonProperty("action_manifest_path")]
    public string ActionManifestPath = null!;

    [JsonProperty("image_path")]
    public string ImagePath = null!;

    [JsonProperty("strings")]
    public VRStrings Strings = new();
}

public class VRStrings
{
    [JsonProperty("en_us")]
    public VRLocalisation Localisation = new();
}

public class VRLocalisation
{
    [JsonProperty("name")]
    public string Name = "VRCOSC";

    [JsonProperty("description")]
    public string Description = "Modular OSC program creator made for VRChat";
}
