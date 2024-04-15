// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.SDK.Modules.Attributes.Types;

namespace VRCOSC.App.Pages.Modules.Settings;

public partial class MutableKeyValuePairSettingPage : INotifyPropertyChanged
{
    public MutableKeyValuePairListModuleSetting ModuleSetting { get; }
    public Visibility ContentVisibility => listModuleSetting.Count() == 0 ? Visibility.Collapsed : Visibility.Visible;

    private readonly IListModuleSetting listModuleSetting;

    public MutableKeyValuePairSettingPage(MutableKeyValuePairListModuleSetting moduleSetting)
    {
        ModuleSetting = moduleSetting;
        listModuleSetting = moduleSetting;

        InitializeComponent();

        DataContext = this;
    }

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var instance = element.Tag;

        listModuleSetting.Remove(instance);
        OnPropertyChanged(nameof(ContentVisibility));
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        listModuleSetting.Add();
        OnPropertyChanged(nameof(ContentVisibility));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
