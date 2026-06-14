// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VRCOSC.App.UI.Views.Nodes.Converters;

public class InputNameVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var renderInline = (bool)values[0];
        var renderName = (bool)values[1];

        return renderInline ? Visibility.Collapsed : renderName ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [];
}