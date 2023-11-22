// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Packages;

/// <summary>
/// Represents the vrcosc.json file remote module listings require to be compatible with VRCOSC
/// </summary>
public class PackageFile
{
    /// <summary>
    /// The ID of this package
    /// </summary>
    [JsonProperty("package_id")]
    public string? PackageID;

    /// <summary>
    /// A display name for the repo screen
    /// </summary>
    [JsonProperty("display_name")]
    public string? DisplayName;

    /// <summary>
    /// A cover image for the repo info overlay
    /// </summary>
    [JsonProperty("cover_image_url")]
    public string? CoverImageUrl;

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
