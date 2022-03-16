// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCOSC.Game.Modules;

public class ModuleData
{
    [JsonProperty("enabled")]
    internal bool Enabled = true;

    [JsonProperty("settings")]
    public Dictionary<string, object> Settings { get; } = new();

    [JsonProperty("parameters")]
    public Dictionary<string, string> Parameters { get; } = new();
}
