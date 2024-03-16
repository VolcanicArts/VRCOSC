// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.Modules;

namespace VRCOSC.App.Pages.Modules;

public partial class ModulesPage : IVRCOSCPage
{
    public ModulesPage()
    {
        InitializeComponent();

        DataContext = ModuleManager.GetInstance();

        AppManager.GetInstance().RegisterPage(PageLookup.Modules, this);
    }

    public void Refresh()
    {
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
