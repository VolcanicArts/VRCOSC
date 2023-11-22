// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Newtonsoft.Json;

namespace VRCOSC.Game.OVR.Metadata;

public class OVRManifest
{
    [JsonProperty("source")]
    public string Source = "builtin";

    [JsonProperty("applications")]
    public OVRApplication[] Applications =
    {
        new()
    };
}

public class OVRApplication
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
    public OVRStrings Strings = new();
}

public class OVRStrings
{
    [JsonProperty("en_us")]
    public OVRLocalisation Localisation = new();
}

public class OVRLocalisation
{
    [JsonProperty("name")]
    public string Name = "VRCOSC";

    [JsonProperty("description")]
    public string Description = "Modular OSC program creator made for VRChat";
}
