// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCOSC.App.OVR;

[JsonObject(MemberSerialization.OptIn)]
public class OpenVRPaths
{
    [JsonProperty("config")]
    public List<string> Config = [];

    [JsonProperty("runtime")]
    public List<string> Runtime = [];
}

[JsonObject(MemberSerialization.OptIn)]
public class SteamVRSettings
{
    [JsonProperty("trackers")]
    public Dictionary<string, string> Trackers = [];
}