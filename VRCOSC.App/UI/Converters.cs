// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VRCOSC.App.UI;

public class IndexOfConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [IList, not null])
        {
            var list = (IList)values[0];
            var item = values[1];

            return list.IndexOf(item);
        }

        return Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public Visibility InvisibleMode { get; init; } = Visibility.Collapsed;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) return Visibility.Visible;

        return boolValue ? Visibility.Visible : InvisibleMode;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class AlternatingColourConverter : IValueConverter
{
    public Brush? Colour1 { get; init; }
    public Brush? Colour2 { get; init; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue) return Brushes.Black;

        return intValue % 2 == 0 ? Colour1 ?? Brushes.Black : Colour2 ?? Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
