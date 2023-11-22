// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Packages.Serialisation;

public class SerialisablePackageManager : SerialisableVersion
{
    [JsonProperty("installed")]
    public List<SerialisablePackageInstall> Installed = new();

    [JsonConstructor]
    public SerialisablePackageManager()
    {
    }

    public SerialisablePackageManager(PackageManager packageManager)
    {
        Version = 1;

        Installed.AddRange(packageManager.InstalledPackages.Select(packageInstall => new SerialisablePackageInstall(packageInstall)));
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

    public SerialisablePackageInstall(PackageInstall packageInstall)
    {
        PackageID = packageInstall.PackageID;
        Version = packageInstall.Version;
    }
}
