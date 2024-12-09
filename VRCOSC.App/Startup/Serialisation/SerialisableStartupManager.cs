// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Startup.Serialisation;

[JsonObject]
public class SerialisableStartupManager : SerialisableVersion
{
    [JsonProperty("instances")]
    public List<SerialisableStartupInstance> Instances { get; set; } = [];

    [JsonConstructor]
    public SerialisableStartupManager()
    {
    }

    public SerialisableStartupManager(StartupManager startupManager)
    {
        Version = 1;

        Instances = startupManager.Instances.Select(startupInstance => new SerialisableStartupInstance(startupInstance)).ToList();
    }
}

[JsonObject]
public class SerialisableStartupInstance
{
    [JsonProperty("file_location")]
    public string FileLocation { get; set; } = string.Empty;

    [JsonProperty("arguments")]
    public string Arguments { get; set; } = string.Empty;

    [JsonConstructor]
    public SerialisableStartupInstance()
    {
    }

    public SerialisableStartupInstance(StartupInstance startupInstance)
    {
        FileLocation = startupInstance.FileLocation.Value;
        Arguments = startupInstance.Arguments.Value;
    }
}