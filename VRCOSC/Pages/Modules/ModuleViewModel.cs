// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using VRCOSC.Modules;

namespace VRCOSC.Pages.Modules;

public class ModuleViewModel
{
    public ObservableDictionary<string, ObservableCollection<Module>> Modules { get; } = new();

    public ModuleViewModel()
    {
        var packageCollection = new ObservableCollection<Module>
        {
            new("test"),
            new("test"),
            new("test"),
            new("test"),
            new("test"),
            new("test")
        };

        Modules.Add("test package", packageCollection);
    }
}
