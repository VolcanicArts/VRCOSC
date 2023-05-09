// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace VRCOSC.Game.Modules.Serialisation.V1.Models;

public class SerialisableModuleManager
{
    [JsonProperty("version")]
    public int Version = 1;

    [JsonProperty("modules")]
    public Dictionary<string, SerialisableModule> Modules = new();

    [JsonConstructor]
    public SerialisableModuleManager()
    {
    }

    public SerialisableModuleManager(IEnumerable<Module> modules)
    {
        modules.ForEach(module => Modules.Add(module.SerialisedName, new SerialisableModule(module)));
    }
}
