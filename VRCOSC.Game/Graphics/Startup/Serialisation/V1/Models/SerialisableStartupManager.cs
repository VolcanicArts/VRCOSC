// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.Startup.Serialisation.V1.Models;

public class SerialisableStartupManager
{
    [JsonProperty("version")]
    public int Verison = 1;

    [JsonProperty("filepaths")]
    public List<string> FilePaths = new();

    [JsonConstructor]
    public SerialisableStartupManager()
    {
    }

    public SerialisableStartupManager(StartupManager startupManager)
    {
        FilePaths = startupManager.FilePaths.Select(path => path.Value).ToList();
    }
}
