// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules.Serialisation.V1.Models;

public class SerialisableModule
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("settings")]
    public Dictionary<string, SerialisableModuleAttribute> Settings = new();

    [JsonProperty("parameters")]
    public Dictionary<string, SerialisableModuleAttribute> Parameters = new();

    [JsonConstructor]
    public SerialisableModule()
    {
    }

    public SerialisableModule(Module module)
    {
        Enabled = module.Enabled.Value;

        module.Settings.ForEach(pair =>
        {
            if (pair.Value.Attribute.IsDefault) return;

            Settings.Add(pair.Key, new SerialisableModuleAttribute(pair.Value));
        });

        module.Parameters.ForEach(pair =>
        {
            if (pair.Value.Attribute.IsDefault) return;

            Parameters.Add(pair.Key.ToLookup(), new SerialisableModuleAttribute(pair.Value));
        });
    }
}
