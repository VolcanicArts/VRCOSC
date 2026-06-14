// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows.Data;

namespace VRCOSC.App.UI.Views.Nodes.Converters;

public class TypeIsStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is Type type && type == typeof(string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}