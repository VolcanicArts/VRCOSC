// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.UI.Views.Modules.Settings.QueryableParameter;

public class QueryableParameterValueEntryVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ComparisonOperation operation)
        {
            return operation != ComparisonOperation.Changed ? Visibility.Visible : Visibility.Collapsed;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class QueryableParameterBoolValueEntryVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ParameterType type)
        {
            return type == ParameterType.Bool ? Visibility.Visible : Visibility.Collapsed;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class QueryableParameterIntValueEntryVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ParameterType type)
        {
            return type == ParameterType.Int ? Visibility.Visible : Visibility.Collapsed;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class QueryableParameterFloatValueEntryVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ParameterType type)
        {
            return type == ParameterType.Float ? Visibility.Visible : Visibility.Collapsed;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class BoolValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? BoolValue.True : BoolValue.False;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BoolValue boolValue)
        {
            return boolValue == BoolValue.True;
        }

        return null;
    }
}