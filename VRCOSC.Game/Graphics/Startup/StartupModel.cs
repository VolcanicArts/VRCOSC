// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Graphics.Startup;

public class StartupModel
{
    [JsonProperty("path")]
    public string Path = null!;

    [JsonConstructor]
    public StartupModel()
    {
    }

    public StartupModel(string path)
    {
        Path = path;
    }
}
