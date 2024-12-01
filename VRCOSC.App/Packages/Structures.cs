// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Octokit;

namespace VRCOSC.App.Packages;

public class PackageRepository
{
    [JsonProperty("description")]
    public string? Description;

    [JsonProperty("default_branch")]
    public string? DefaultBranch;

    [JsonProperty("package_file")]
    public PackageFile? PackageFile { get; set; }

    [JsonProperty("releases")]
    public List<PackageRelease> Releases = new();

    [JsonConstructor]
    public PackageRepository()
    {
    }

    public PackageRepository(Repository repository)
    {
        Description = repository.Description;
        DefaultBranch = repository.DefaultBranch;
    }
}

public class PackageFile
{
    [JsonProperty("package_id")]
    public string? PackageID;

    [JsonProperty("display_name")]
    public string? DisplayName;

    [JsonProperty("cover_image_url")]
    public string? CoverImageUrl;
}

public class PackageRelease
{
    [JsonProperty("version")]
    public string Version { get; set; } = null!;

    [JsonProperty("prerelease")]
    public bool IsPreRelease { get; set; }

    [JsonProperty("assets")]
    public List<string> Assets = new();

    [JsonConstructor]
    public PackageRelease()
    {
    }

    public PackageRelease(Release release)
    {
        Version = release.TagName;
        IsPreRelease = release.Prerelease;
        Assets = release.Assets.Where(asset => asset.Name.EndsWith(".dll") || asset.Name.EndsWith(".zip")).Select(asset => asset.Name).ToList();
    }
}