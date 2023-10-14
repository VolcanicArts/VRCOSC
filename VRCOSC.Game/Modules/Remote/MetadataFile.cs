// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Remote;

public class MetadataFile
{
    [JsonProperty("installed_version")]
    public string InstalledVersion = null!;
}
