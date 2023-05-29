// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Extensions.IEnumerableExtensions;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Modules.Serialisation.Legacy.Models;

public class LegacySerialisableModuleManager
{
    [JsonProperty("version")]
    public int Version;

    [JsonProperty("modules")]
    public Dictionary<string, LegacySerialisableModule> Modules = new();

    [JsonConstructor]
    public LegacySerialisableModuleManager()
    {
    }

    public LegacySerialisableModuleManager(ModuleManager moduleManager)
    {
        Version = 1;
        moduleManager.ForEach(module => Modules.Add(module.SerialisedName, new LegacySerialisableModule(module)));
    }
}
