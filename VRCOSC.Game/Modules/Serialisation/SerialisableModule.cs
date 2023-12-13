// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.SDK;
using VRCOSC.Serialisation;

namespace VRCOSC.Modules.Serialisation;

public class SerialisableModule : SerialisableVersion
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("settings")]
    public Dictionary<string, object> Settings = new();

    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters = new();

    [JsonConstructor]
    public SerialisableModule()
    {
    }

    public SerialisableModule(Module module)
    {
        Version = 1;

        Enabled = module.Enabled.Value;
        module.Settings.Where(pair => !pair.Value.IsDefault()).ForEach(pair => Settings.Add(pair.Key, pair.Value.GetRawValue()));
        module.Parameters.Where(pair => !pair.Value.IsDefault()).ForEach(pair => Parameters.Add(pair.Key.ToLookup(), pair.Value.GetRawValue()));
    }
}
