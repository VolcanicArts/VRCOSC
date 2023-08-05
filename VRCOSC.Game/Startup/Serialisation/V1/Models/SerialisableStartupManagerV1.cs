// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Startup.Serialisation.V1.Models;

public class SerialisableStartupManagerV1
{
    [JsonProperty("version")]
    public int Verison;

    [JsonProperty("filepaths")]
    public List<string> FilePaths = new();

    [JsonConstructor]
    public SerialisableStartupManagerV1()
    {
    }

    public SerialisableStartupManagerV1(StartupManager startupManager)
    {
        Verison = 1;
        FilePaths = startupManager.Instances.Select(instance => instance.FilePath.Value).ToList();
    }
}
