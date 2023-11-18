// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Serialisation;

public class SerialisableModule : SerialisableVersion
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("settings")]
    public Dictionary<string, object> Settings = new();

    [JsonConstructor]
    public SerialisableModule()
    {
    }

    public SerialisableModule(Module module)
    {
        Version = 1;

        Enabled = module.Enabled.Value;

        module.Settings.ForEach(pair =>
        {
            if (pair.Value.IsDefault()) return;

            Settings.Add(pair.Key, pair.Value.GetRawValue());
        });
    }
}
