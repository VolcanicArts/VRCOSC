// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.App.OpenVR.Metadata;

internal class OVRManifest
{
    [JsonProperty("source")]
    public string Source = "builtin";

    [JsonProperty("applications")]
    public OVRApplication[] Applications = [new()];
}

internal class OVRApplication
{
    [JsonProperty("app_key")]
    public string AppKey = "volcanicarts.vrcosc";

    [JsonProperty("launch_type")]
    public string LaunchType = "binary";

    [JsonProperty("binary_path_windows")]
    public string BinaryPathWindows = null!;

    [JsonProperty("is_dashboard_overlay")]
    public bool IsDashboardOverlay = true;

    [JsonProperty("action_manifest_path")]
    public string ActionManifestPath = null!;

    [JsonProperty("image_path")]
    public string ImagePath = null!;

    [JsonProperty("strings")]
    public OVRStrings Strings = new();
}

internal class OVRStrings
{
    [JsonProperty("en_us")]
    public OVRLocalisation Localisation = new();
}

internal class OVRLocalisation
{
    [JsonProperty("name")]
    public string Name = "VRCOSC";

    [JsonProperty("description")]
    public string Description = "Modular OSC program creator, toolkit, and router made for VRChat";
}