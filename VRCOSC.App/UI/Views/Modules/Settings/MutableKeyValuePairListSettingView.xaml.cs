// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.SDK.Modules.Attributes.Types;

namespace VRCOSC.App.UI.Views.Modules.Settings;

public partial class MutableKeyValuePairListSettingView
{
    public MutableKeyValuePairListModuleSetting ModuleSetting { get; }

    public MutableKeyValuePairListSettingView(MutableKeyValuePairListModuleSetting moduleSetting)
    {
        ModuleSetting = moduleSetting;

        InitializeComponent();

        DataContext = this;
    }

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var instance = element.Tag;

        ModuleSetting.Remove(instance);
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        ModuleSetting.Add();
    }
}
