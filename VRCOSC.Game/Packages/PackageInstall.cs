// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Packages.Serialisation;

namespace VRCOSC.Game.Packages;

public class PackageInstall
{
    public readonly string PackageID;
    public readonly string Version;

    public PackageInstall(string packageID, string version)
    {
        PackageID = packageID;
        Version = version;
    }

    public PackageInstall(SerialisablePackageInstall install)
    {
        PackageID = install.PackageID;
        Version = install.Version;
    }
}
