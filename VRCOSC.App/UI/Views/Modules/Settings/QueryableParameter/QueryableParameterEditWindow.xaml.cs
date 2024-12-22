// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.Parameters.Queryable;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterEditWindow : IManagedWindow
{
    public IEnumerable<ParameterType> QueryableParameterTypeItemsSource => typeof(ParameterType).GetEnumValues().Cast<ParameterType>();
    public IEnumerable<BoolValue> BoolValueItemsSource => typeof(BoolValue).GetEnumValues().Cast<BoolValue>();
    public Array QueryableParameterActionTypeItemsSource => QueryableParameterList.ActionTypeSource;

    public QueryableParameterList QueryableParameterList { get; }

    public QueryableParameterEditWindow(QueryableParameterList queryableParameterList)
    {
        InitializeComponent();
        QueryableParameterList = queryableParameterList;
        DataContext = this;
        refreshParameterList();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        QueryableParameterList.Add();
        refreshParameterList();
    }

    private void refreshParameterList()
    {
        ParameterList.ItemsSource = null;
        ParameterList.ItemsSource = QueryableParameterList.Parameters;
    }

    public object GetComparer() => QueryableParameterList;

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var instance = element.Tag;

        QueryableParameterList.Remove(instance);
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