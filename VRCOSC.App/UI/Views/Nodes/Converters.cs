// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes;

public enum NodeIoLocation
{
    Input,
    Output,
    Both
}

public enum NodeIoType
{
    Flow,
    Value
}

public class FlowVisibilityConverter : IValueConverter
{
    public NodeIoLocation NodeIoLocation { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return null;
        if (!node.GetType().TryGetCustomAttribute<NodeFlowAttribute>(out var attribute)) return Visibility.Collapsed;

        return NodeIoLocation switch
        {
            NodeIoLocation.Input => !attribute.IsTrigger ? Visibility.Visible : Visibility.Collapsed,
            NodeIoLocation.Output => attribute.NumFlowOutputs > 0 ? Visibility.Visible : Visibility.Collapsed,
            NodeIoLocation.Both => !attribute.IsTrigger || attribute.NumFlowOutputs > 0 ? Visibility.Visible : Visibility.Collapsed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class FlowAmountConverter : IValueConverter
{
    public NodeIoLocation NodeIoLocation { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return null;
        if (!node.GetType().TryGetCustomAttribute<NodeFlowAttribute>(out var attribute)) return Array.Empty<int>();

        return NodeIoLocation switch
        {
            NodeIoLocation.Input => !attribute.IsTrigger ? Enumerable.Range(0, 1) : Array.Empty<int>(),
            NodeIoLocation.Output => attribute.NumFlowOutputs > 0 ? Enumerable.Range(0, attribute.NumFlowOutputs) : Array.Empty<int>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class ValueVisibilityConverter : IValueConverter
{
    public NodeIoLocation NodeIoLocation { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return null;
        if (!node.GetType().TryGetCustomAttribute<NodeValueAttribute>(out var attribute)) return Visibility.Collapsed;

        var metadata = node.NodeScape.Metadata[node.Id];

        return NodeIoLocation switch
        {
            NodeIoLocation.Input => metadata.ValueInputTypeCount > 0 ? Visibility.Visible : Visibility.Collapsed,
            NodeIoLocation.Output => metadata.ValueOutputTypes.Length > 0 ? Visibility.Visible : Visibility.Collapsed,
            NodeIoLocation.Both => metadata.ValueInputTypeCount > 0 || metadata.ValueOutputTypes.Length > 0 ? Visibility.Visible : Visibility.Collapsed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class ValueAmountConverter : IValueConverter
{
    public NodeIoLocation NodeIoLocation { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return null;
        if (!node.GetType().TryGetCustomAttribute<NodeValueAttribute>(out var attribute)) return Array.Empty<int>();

        var metadata = node.NodeScape.Metadata[node.Id];

        return NodeIoLocation switch
        {
            NodeIoLocation.Input => metadata.ValueInputTypeCount > 0 ? Enumerable.Range(0, metadata.ValueInputTypeCount) : Array.Empty<int>(),
            NodeIoLocation.Output => metadata.ValueOutputTypes.Length > 0 ? Enumerable.Range(0, metadata.ValueOutputTypes.Length) : Array.Empty<int>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class SlotNameConverter : IValueConverter
{
    public NodeIoLocation NodeIoLocation { get; set; }
    public NodeIoType NodeIoType { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int slot) return null;

        switch (NodeIoLocation)
        {
            case NodeIoLocation.Output when NodeIoType == NodeIoType.Flow:
                return $"flow_output_{slot}";

            case NodeIoLocation.Output when NodeIoType == NodeIoType.Value:
                return $"value_output_{slot}";

            case NodeIoLocation.Input when NodeIoType == NodeIoType.Flow:
                return $"flow_input_{slot}";

            case NodeIoLocation.Input when NodeIoType == NodeIoType.Value:
                return $"value_input_{slot}";

            default:
                return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}