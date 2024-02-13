// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Semver;
using VRCOSC.Packages;

namespace VRCOSC.Pages.Packages;

public class PackageViewModel : INotifyPropertyChanged
{
    private double scrollHeight = double.NaN;

    public double ScrollHeight
    {
        get => scrollHeight;
        set
        {
            scrollHeight = value;
            OnPropertyChanged();
        }
    }

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
