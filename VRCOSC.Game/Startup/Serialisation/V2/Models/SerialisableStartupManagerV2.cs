// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Startup.Serialisation.V2.Models;

public class SerialisableStartupManagerV2
{
    [JsonProperty("version")]
    public int Verison;

    [JsonProperty("instances")]
    public List<SerialisableStartupInstance> Instances = new();

    [JsonConstructor]
    public SerialisableStartupManagerV2()
    {
    }

    public SerialisableStartupManagerV2(StartupManager startupManager)
    {
        Verison = 2;
        Instances = startupManager.Instances.Select(instance => new SerialisableStartupInstance(instance)).ToList();
    }
}

public class SerialisableStartupInstance
{
    [JsonProperty("file_path")]
    public string FilePath = string.Empty;

    [JsonProperty("launch_arguments")]
    public string LaunchArguments = string.Empty;

    [JsonConstructor]
    public SerialisableStartupInstance()
    {
    }

    public SerialisableStartupInstance(StartupInstance instance)
    {
        FilePath = instance.FilePath.Value;
        LaunchArguments = instance.LaunchArguments.Value;
    }
}
