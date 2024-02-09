// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.Windows;
using Semver;
using VRCOSC.Packages;

namespace VRCOSC.Pages.Packages;

public class PackageViewModel
{
    public ObservableCollection<Package> Packages { get; } = new();

    public RelayCommand<string> InstallButtonCommand { get; }

    public PackageViewModel()
    {
        InstallButtonCommand = new RelayCommand<string>(installButtonCommand);

        Packages.Add(new Package("foo", "Test", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Official));
        Packages.Add(new Package("bar", "Test2", new SemVersion(1, 1, 0), new SemVersion(1, 0, 0), PackageType.Community));
    }

    private void installButtonCommand(string tag)
    {
        MessageBox.Show(tag);
    }
}
