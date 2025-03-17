// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VRCOSC.App.SDK.Parameters;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterView
{
    public QueryableParameterView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = QueryableParameter;
    }

    public IEnumerable<ParameterType> QueryableParameterTypeItemsSource => typeof(ParameterType).GetEnumValues().Cast<ParameterType>();
    public IEnumerable<BoolValue> BoolValueItemsSource => typeof(BoolValue).GetEnumValues().Cast<BoolValue>();

    public static readonly DependencyProperty QueryableParameterProperty =
        DependencyProperty.Register(nameof(QueryableParameter), typeof(object), typeof(QueryableParameterView), new PropertyMetadata(null));

    public object QueryableParameter
    {
        get => GetValue(QueryableParameterProperty);
        set => SetValue(QueryableParameterProperty, value);
    }
}