// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Parameters;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.App.Pages.Modules.Parameters;

public sealed partial class ModuleParametersWindow : INotifyPropertyChanged
{
    public Module Module { get; }

    public List<ModuleParameter> UIParameters => Module.Parameters.Select(pair => pair.Value).ToList();
    public List<ModuleSetting> UISettings => Module.Settings.Select(pair => pair.Value).ToList();

    public ModuleParametersWindow(Module module)
    {
        InitializeComponent();

        Title = $"{module.Title.Pluralise()} Parameters";

        Module = module;
        DataContext = this;

        SizeChanged += OnSizeChanged;
    }

    private double parameterScrollViewerHeight = double.NaN;

    public double ParameterScrollViewerHeight
    {
        get => parameterScrollViewerHeight;
        set
        {
            parameterScrollViewerHeight = value;
            OnPropertyChanged();
        }
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        evaluateContentHeight();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => evaluateContentHeight();

    private void evaluateContentHeight()
    {
        if (UIParameters.Count == 0)
        {
            ParameterScrollViewerHeight = 0;
            return;
        }

        var contentHeight = ParameterListView.ActualHeight;
        var targetHeight = GridContainer.ActualHeight - 50;
        ParameterScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void ResetParameters_OnClick(object sender, RoutedEventArgs e)
    {
        Module.Parameters.Values.ForEach(parameter => parameter.SetDefault());
    }
}

public class BackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue) return Brushes.Black;

        return intValue % 2 == 0 ? (Brush)Application.Current.Resources["CBackground3"] : (Brush)Application.Current.Resources["CBackground4"];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
