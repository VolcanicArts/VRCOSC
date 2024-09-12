// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Core;

public class BoolToVisibilityConverter : IValueConverter
{
    public Visibility InvisibleMode { get; init; } = Visibility.Collapsed;
    public bool Invert { get; init; } = false;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) return InvisibleMode;

        return Invert ? !boolValue ? Visibility.Visible : InvisibleMode : boolValue ? Visibility.Visible : InvisibleMode;
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

public class RectSizeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double width && values[1] is double height)
        {
            return new Rect(0, 0, width, height);
        }

        return new Rect();
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}

public class HeaderFooterListViewContentHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double listDesiredHeight && values[1] is double totalHeight && values[2] is double headerHeight && values[3] is double footerHeight && values[4] is bool shouldTruncateHeight && values[5] is bool overrideCollapse)
        {
            if (overrideCollapse) return 0d;
            if (!shouldTruncateHeight) return double.NaN;

            var targetHeight = totalHeight - headerHeight - footerHeight;
            var height = listDesiredHeight >= targetHeight ? targetHeight : double.NaN;
            return height;
        }

        return 0;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}

public class HeaderFooterListViewPanningModeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double listDesiredHeight && values[1] is double totalHeight && values[2] is double headerHeight && values[3] is double footerHeight && values[4] is bool shouldTruncateHeight)
        {
            if (!shouldTruncateHeight) return PanningMode.None;

            var targetHeight = totalHeight - headerHeight - footerHeight;
            return listDesiredHeight >= targetHeight ? PanningMode.VerticalOnly : PanningMode.None;
        }

        return PanningMode.None;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}

public class TypeToReadableTypeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Type type)
        {
            return type.ToReadableName();
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
