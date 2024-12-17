// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterEditWindow : IManagedWindow
{
    public IEnumerable<ParameterType> QueryableParameterTypeItemsSource => typeof(ParameterType).GetEnumValues().Cast<ParameterType>();
    public IEnumerable<KeyValuePair<string, ComparisonOperation>> QueryableParameterOperationItemsSource => ComparisonOperationUtils.DISPLAY_LIST;
    public IEnumerable<BoolValue> BoolValueItemsSource => typeof(BoolValue).GetEnumValues().Cast<BoolValue>();
    public Array QueryableParameterActionTypeItemsSource { get; init; } = Array.Empty<object>();

    public ListModuleSetting ModuleSetting { get; }

    public QueryableParameterEditWindow(QueryableParameterListModuleSetting moduleSetting)
    {
        InitializeComponent();
        ModuleSetting = moduleSetting;
        DataContext = this;

        Title = $"Edit {moduleSetting.Title.Pluralise()} Queryable Parameters";
    }

    public QueryableParameterEditWindow(ActionableQueryableParameterListModuleSetting moduleSetting)
    {
        InitializeComponent();
        ModuleSetting = moduleSetting;
        DataContext = this;

        Title = $"Edit {moduleSetting.Title.Pluralise()} Queryable Parameters";

        QueryableParameterActionTypeItemsSource = moduleSetting.ActionType.GetEnumValues();
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        ModuleSetting.Add();
    }

    public object GetComparer() => ModuleSetting;
}

public enum BoolValue
{
    True,
    False
}