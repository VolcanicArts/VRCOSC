// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using Semver;
using VRCOSC.Packages;

namespace VRCOSC.Pages.Packages;

public class PackageViewModel
{
    public ObservableCollection<Package> Packages { get; } = new();

    public PackageViewModel()
    {
        Packages.Add(new Package("foo", "Test", new SemVersion(1, 1, 0), null, PackageType.Official));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
    }
}
