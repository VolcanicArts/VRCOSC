﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules.Serialisation;

public class SerialisableModule : SerialisableVersion
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("settings")]
    public Dictionary<string, object?> Settings = new();

    [JsonProperty("parameters")]
    public Dictionary<string, SerialisableParameter> Parameters = new();

    [JsonConstructor]
    public SerialisableModule()
    {
    }

    public SerialisableModule(Module module)
    {
        Version = 1;

        Enabled = module.Enabled.Value;
        module.Settings.Where(pair => !pair.Value.InternalIsDefault()).ForEach(pair => Settings.Add(pair.Key, pair.Value.InternalSerialise()));
        module.Parameters.Where(pair => !pair.Value.IsDefault()).ForEach(pair => Parameters.Add(pair.Key.ToLookup(), new SerialisableParameter(pair.Value)));
    }
}

public class SerialisableParameter
{
    [JsonProperty("enabled")]
    public bool Enabled;

    [JsonProperty("parameter_name")]
    public string ParameterName;

    public SerialisableParameter()
    {
    }

    public SerialisableParameter(ModuleParameter moduleParameter)
    {
        Enabled = moduleParameter.Enabled.Value;
        ParameterName = moduleParameter.Name.Value;
    }
}