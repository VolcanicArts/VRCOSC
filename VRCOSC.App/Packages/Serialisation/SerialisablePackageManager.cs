// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Packages.Serialisation;

public class SerialisablePackageManager : SerialisableVersion
{
    [JsonProperty("installed")]
    public List<SerialisablePackageInstall> Installed = new();

    [JsonProperty("cache_expire_time")]
    public DateTimeOffset CacheExpireTime;

    [JsonProperty("cache")]
    public List<SerialisablePackageSource> Cache = new();

    [JsonConstructor]
    public SerialisablePackageManager()
    {
    }

    public SerialisablePackageManager(PackageManager packageManager)
    {
        Version = 1;
        Installed.AddRange(packageManager.InstalledPackages.Select(packageInstall => new SerialisablePackageInstall(packageInstall)));
        CacheExpireTime = packageManager.CacheExpireTime;
        Cache.AddRange(packageManager.Sources.Select(packageSource => new SerialisablePackageSource(packageSource)));
    }
}

public class SerialisablePackageInstall
{
    [JsonProperty("package_id")]
    public string PackageID = null!;

    [JsonProperty("version")]
    public string Version = null!;

    [JsonConstructor]
    public SerialisablePackageInstall()
    {
    }

    public SerialisablePackageInstall(KeyValuePair<string, string> pair)
    {
        PackageID = pair.Key;
        Version = pair.Value;
    }
}

public class SerialisablePackageSource
{
    [JsonIgnore]
    public string Reference => $"{Owner}#{Name}";

    [JsonProperty("owner")]
    public string Owner = null!;

    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("repository")]
    public PackageRepository? Repository;

    [JsonConstructor]
    public SerialisablePackageSource()
    {
    }

    public SerialisablePackageSource(PackageSource packageSource)
    {
        Owner = packageSource.RepoOwner;
        Name = packageSource.RepoName;
        Repository = packageSource.Repository;
    }
}