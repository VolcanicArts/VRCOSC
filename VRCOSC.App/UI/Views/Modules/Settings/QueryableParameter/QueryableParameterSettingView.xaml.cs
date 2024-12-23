// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using VRCOSC.App.SDK.Parameters.Queryable;
using VRCOSC.App.UI.Core;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public partial class QueryableParameterSettingView
{
    private WindowManager windowManager = null!;

    public static readonly DependencyProperty QueryableParameterListProperty =
        DependencyProperty.Register(nameof(QueryableParameterList), typeof(QueryableParameterList), typeof(QueryableParameterSettingView), new PropertyMetadata(null));

    public QueryableParameterList QueryableParameterList
    {
        get => (QueryableParameterList)GetValue(QueryableParameterListProperty);
        set => SetValue(QueryableParameterListProperty, value);
    }

    public QueryableParameterSettingView()
    {
        InitializeComponent();
    }

    private void QueryableParameterSettingView_OnLoaded(object sender, RoutedEventArgs e)
    {
        windowManager = new WindowManager(this);
    }

    private void EditButton_OnClick(object sender, RoutedEventArgs e)
    {
        windowManager.TrySpawnChild(new QueryableParameterEditWindow(QueryableParameterList));
    }
}