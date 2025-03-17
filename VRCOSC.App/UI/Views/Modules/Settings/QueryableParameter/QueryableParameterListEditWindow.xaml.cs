// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterListEditWindow : IManagedWindow
{
    public ListModuleSetting ModuleSetting { get; }
    public IEnumerable QueryableParameterList => ModuleSetting.GetEnumerable();

    public QueryableParameterListEditWindow(ListModuleSetting moduleSetting)
    {
        ModuleSetting = moduleSetting;
        InitializeComponent();
        DataContext = this;
        refreshParameterList();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        ModuleSetting.Add();
        refreshParameterList();
    }

    private void refreshParameterList()
    {
        ParameterList.ItemsSource = null;
        ParameterList.ItemsSource = QueryableParameterList;
    }

    public object GetComparer() => QueryableParameterList;

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var instance = element.Tag;

        ModuleSetting.Remove(instance);
        refreshParameterList();
    }
}

public class ComparisonComboBoxItemsSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ParameterType type)
        {
            return type == ParameterType.Bool ? ComparisonOperationUtils.BOOL_DISPLAY_LIST : ComparisonOperationUtils.VALUE_DISPLAY_LIST;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public enum BoolValue
{
    True,
    False
}