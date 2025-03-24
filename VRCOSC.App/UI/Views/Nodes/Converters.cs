// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.UI.Views.Nodes;

public class ConnectionAmountConverter : IValueConverter
{
    public ConnectionSide ConnectionSide { get; set; }
    public ConnectionType ConnectionType { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) throw new Exception($"{nameof(ConnectionAmountConverter)} must take in a node for the binding");

        if (ConnectionType == ConnectionType.Flow && node.IsFlowNode(ConnectionSide.Input | ConnectionSide.Output))
        {
            if (ConnectionSide == ConnectionSide.Input && node.IsFlowNode(ConnectionSide.Input))
            {
                return !node.GetFlowInputAttribute().IsTrigger ? Enumerable.Range(0, 1) : Array.Empty<int>();
            }

            if (ConnectionSide == ConnectionSide.Output && node.IsFlowNode(ConnectionSide.Output))
            {
                var count = node.GetFlowOutputAttribute().Count;
                return count > 0 ? Enumerable.Range(0, count) : Array.Empty<int>();
            }
        }

        if (ConnectionType == ConnectionType.Value && node.IsValueNode(ConnectionSide.Input | ConnectionSide.Output))
        {
            if (ConnectionSide == ConnectionSide.Input && node.IsValueNode(ConnectionSide.Input))
            {
                var count = node.GetValueInputAttribute().Count;
                return count > 0 ? Enumerable.Range(0, count) : Array.Empty<int>();
            }

            if (ConnectionSide == ConnectionSide.Output && node.IsValueNode(ConnectionSide.Output))
            {
                var count = node.GetValueOutputAttribute().Count;
                return count > 0 ? Enumerable.Range(0, count) : Array.Empty<int>();
            }
        }

        return Array.Empty<int>();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class SlotNameConverter : IValueConverter
{
    public ConnectionSide ConnectionSide { get; set; }
    public ConnectionType ConnectionType { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int slot) return null;

        switch (ConnectionSide)
        {
            case ConnectionSide.Output when ConnectionType == ConnectionType.Flow:
                return $"flow_output_{slot}";

            case ConnectionSide.Output when ConnectionType == ConnectionType.Value:
                return $"value_output_{slot}";

            case ConnectionSide.Input when ConnectionType == ConnectionType.Flow:
                return $"flow_input_{slot}";

            case ConnectionSide.Input when ConnectionType == ConnectionType.Value:
                return $"value_input_{slot}";

            default:
                return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}