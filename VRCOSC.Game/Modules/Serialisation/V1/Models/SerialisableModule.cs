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

    [JsonProperty("settings_single")]
    public Dictionary<string, SerialisableModuleAttributeSingle> SettingsSingle = new();

    [JsonProperty("settings_list")]
    public Dictionary<string, SerialisableModuleAttributeList> SettingsList = new();

    [JsonConstructor]
    public SerialisableModule()
    {
    }

    public SerialisableModule(Module module)
    {
        Enabled = module.Enabled.Value;

        module.Settings.ForEach(pair =>
        {
            if (pair.Value is ModuleAttributeSingle)
            {
            }

            if (pair.Value is ModuleAttributeList)
            {

            }
        });
    }
}
