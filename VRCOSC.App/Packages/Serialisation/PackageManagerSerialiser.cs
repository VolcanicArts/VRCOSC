﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Packages.Serialisation;

public class PackageManagerSerialiser : Serialiser<PackageManager, SerialisablePackageManager>
{
    protected override string Directory => "configuration";
    protected override string FileName => "packages.json";

    public PackageManagerSerialiser(Storage storage, PackageManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisablePackageManager data)
    {
        data.Installed.ForEach(packageInstall => Reference.InstalledPackages[packageInstall.PackageID] = packageInstall.Version);

        Reference.CacheExpireTime = data.CacheExpireTime;

        data.Cache.ForEach(serialisablePackageSource =>
        {
            var packageSource = Reference.Sources.SingleOrDefault(packageSource => packageSource.InternalReference == serialisablePackageSource.Reference);

            if (packageSource is null)
            {
                var newPackageSource = new PackageSource(Reference, serialisablePackageSource.Owner, serialisablePackageSource.Name);
                newPackageSource.InjectCachedData(serialisablePackageSource.Repository!);
                Reference.Sources.Add(newPackageSource);
            }
            else
            {
                packageSource.InjectCachedData(serialisablePackageSource.Repository!);
            }
        });

        return false;
    }
}