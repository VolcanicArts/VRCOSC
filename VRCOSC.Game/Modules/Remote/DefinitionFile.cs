// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Remote;

/// <summary>
/// Represents the vrcosc.json file remote module listings require to be compatible with VRCOSC
/// </summary>
public class DefinitionFile
{
    /// <summary>
    /// The display name of this collection of modules
    /// </summary>
    [JsonProperty("collection_name")]
    public string CollectionName = null!;

    /// <summary>
    /// A semver string to be analysed when loading this collection
    /// </summary>
    [JsonProperty("sdk_version_range")]
    public string SDKVersionRange = null!;

    /// <summary>
    /// The remote files of this collection.
    /// These files get published alongside vrcosc.json so should be file names
    /// </summary>
    [JsonProperty("files")]
    public string[] Files = null!;
}
