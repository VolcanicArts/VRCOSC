// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.Modules;

namespace VRCOSC.App.Pages.Modules.Parameters;

public partial class ModuleParametersWindow
{
    private readonly Module module;

    public ModuleParametersWindow(Module module)
    {
        InitializeComponent();

        Title = module.Title + "'s Parameters";

        this.module = module;
        DataContext = module;

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => evaluateContentHeight();

    private void evaluateContentHeight()
    {
        if (module.UIParameters.Count == 0)
        {
            module.ParameterScrollViewerHeight = 0;
            return;
        }

        var contentHeight = ParameterListView.ActualHeight;
        var targetHeight = GridContainer.ActualHeight - 55;
        module.ParameterScrollViewerHeight = contentHeight >= targetHeight ? targetHeight : double.NaN;
    }
}

public class BackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var index = System.Convert.ToInt32(value);
        return index % 2 == 0 ? Application.Current.Resources["CBackground3"] : Application.Current.Resources["CBackground4"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
