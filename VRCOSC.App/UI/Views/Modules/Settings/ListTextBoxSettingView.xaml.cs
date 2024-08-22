// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;

namespace VRCOSC.App.UI.Views.Modules.Settings;

public partial class ListTextBoxSettingView
{
    public ModuleSetting ModuleSetting { get; }

    private readonly ListModuleSetting listModuleSetting;

    public ListTextBoxSettingView(Module _, ListModuleSetting moduleSetting)
    {
        listModuleSetting = moduleSetting;
        ModuleSetting = moduleSetting;

        InitializeComponent();

        DataContext = this;
    }

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var instance = element.Tag;

        listModuleSetting.Remove(instance);
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        listModuleSetting.Add();
    }
}
