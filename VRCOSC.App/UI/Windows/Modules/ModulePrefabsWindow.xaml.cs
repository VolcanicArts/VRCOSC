// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Windows;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.Modules;

public partial class ModulePrefabsWindow
{
    public IEnumerable<ModulePrefabAttribute> Prefabs { get; }

    public ModulePrefabsWindow(Module module)
    {
        Prefabs = module.Prefabs;
        Title = $"{module.Title.Pluralise()} Prefabs";
        DataContext = this;

        InitializeComponent();
    }

    private void DownloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var prefabAttribute = (ModulePrefabAttribute)element.Tag;

        prefabAttribute.Url.OpenExternally();
    }
}
