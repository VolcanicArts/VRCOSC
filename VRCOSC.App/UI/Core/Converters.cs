// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using VRCOSC.App.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.App.UI.Core;

public class NullToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

/// <inheritdoc />
/// <summary>
/// Converts a boolean to the set visibility values
/// </summary>
/// <remarks>One Way</remarks>
public class BoolToVisibilityConverter : IValueConverter
{
    public Visibility True { get; init; } = Visibility.Visible;
    public Visibility False { get; init; } = Visibility.Collapsed;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) return False;

        return boolValue ? True : False;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

/// <inheritdoc />
/// <summary>
/// Converts a boolean to the set thickness values
/// </summary>
/// <remarks>One Way</remarks>
public class BoolToThicknessConverter : IValueConverter
{
    public Thickness True { get; init; } = new(1);
    public Thickness False { get; init; } = new(0);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue) throw new Exception($"{nameof(value)} is not a {nameof(Boolean)}");

        return boolValue ? True : False;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class StringToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string strValue) return Visibility.Collapsed;

        return string.IsNullOrEmpty(strValue) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class EnumItemSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return null;
        if (!value.GetType().IsAssignableTo(typeof(Enum))) return null;

        return Enum.GetValues(value.GetType());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

/// <inheritdoc />
/// <summary>
/// Takes in an integer and converts it to the set colours based on if it's even
/// </summary>
/// <remarks>One Way</remarks>
public class AlternatingColourConverter : IValueConverter
{
    public Brush Colour1 { get; init; } = Brushes.Aqua;
    public Brush Colour2 { get; init; } = Brushes.Aqua;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue) throw new Exception($"{nameof(value)} is not an {nameof(Int32)}");

        return intValue % 2 == 0 ? Colour1 : Colour2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class AlternatingColourConverterMulti : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [int intValue, Brush colour1, Brush colour2]) return Brushes.Aqua;

        return intValue % 2 == 0 ? colour1 : colour2;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => [];
}

/// <inheritdoc />
/// <summary>
/// Converts a <see cref="T:System.Type" /> in a friendlier type name
/// </summary>
/// <remarks>One Way</remarks>
public class TypeToFriendlyNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Type typeValue) throw new Exception($"{nameof(value)} is not a {nameof(Type)}");

        return typeValue.GetFriendlyName();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class ParameterInfoToFriendlyNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ParameterInfo parameterInfo) throw new Exception($"{nameof(value)} is not a {nameof(ParameterInfo)}");

        return parameterInfo.GetFriendlyName();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

/// <inheritdoc />
/// <summary>
/// Finds the index of the list view item from the parent list view
/// </summary>
/// <remarks>One Way</remarks>
public class IndexFromListConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ListViewItem listViewItem) throw new Exception($"{nameof(value)} is not a {nameof(ListViewItem)}");

        var listView = ItemsControl.ItemsControlFromItemContainer(listViewItem) as ListView;
        return listView?.ItemContainerGenerator.IndexFromContainer(listViewItem) ?? -1;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

/// <inheritdoc />
/// <summary>
/// Converts between \n for in code and <see cref="P:System.Environment.NewLine" /> for in WPF TextBoxes
/// </summary>
/// <remarks>Two Way</remarks>
public class TextBoxParsingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string str ? str.Replace("\\n", Environment.NewLine) : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string str ? str.Replace(Environment.NewLine, "\\n") : value;
}

public class BorderClipConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [double width, double height, CornerRadius radius]) return DependencyProperty.UnsetValue;

        if (width < double.Epsilon || height < double.Epsilon) return Geometry.Empty;

        var clip = new RectangleGeometry(new Rect(0, 0, width, height), radius.TopLeft, radius.TopLeft);
        clip.Freeze();

        return clip;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}