// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Nodes.Types.Converters;

namespace VRCOSC.App.UI.Views.Nodes;

public record ConnectionViewModel(int Slot, string Title, Type? Type);

public class ConnectionAmountConverter : IValueConverter
{
    public ConnectionSide ConnectionSide { get; set; }
    public ConnectionType ConnectionType { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return null;

        var nodeScape = node.NodeScape;
        var metadata = nodeScape.GetMetadata(node);

        if (ConnectionType == ConnectionType.Flow && node.IsFlowNode(ConnectionSide.Input | ConnectionSide.Output))
        {
            if (ConnectionSide == ConnectionSide.Input)
            {
                if (metadata.Trigger is not null) return Array.Empty<ConnectionViewModel>();
                if (!node.IsFlowNode(ConnectionSide.Input)) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, node.InputFlows.Count).Select(i => new ConnectionViewModel(i, node.InputFlows[i].Name, null));
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!node.IsFlowNode(ConnectionSide.Output)) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, node.OutputFlows.Count).Select(i => new ConnectionViewModel(i, node.OutputFlows[i].Name, null));
            }
        }

        if (ConnectionType == ConnectionType.Value && node.IsValueNode(ConnectionSide.Input | ConnectionSide.Output))
        {
            if (ConnectionSide == ConnectionSide.Input)
            {
                if (!node.IsValueNode(ConnectionSide.Input)) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, metadata.ValueInputNames.Length).Select(i => new ConnectionViewModel(i, metadata.ValueInputNames[i], metadata.Process.InputTypes[i]));
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!node.IsValueNode(ConnectionSide.Output)) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, metadata.ValueOutputNames.Length).Select(i => new ConnectionViewModel(i, metadata.ValueOutputNames[i], metadata.Process.OutputTypes[i]));
            }
        }

        return Array.Empty<ConnectionViewModel>();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class SlotNameConverter : IValueConverter
{
    public ConnectionSide ConnectionSide { get; set; }
    public ConnectionType ConnectionType { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ConnectionViewModel connectionViewModel) return null;

        switch (ConnectionSide)
        {
            case ConnectionSide.Output when ConnectionType == ConnectionType.Flow:
                return $"flow_output_{connectionViewModel.Slot}";

            case ConnectionSide.Output when ConnectionType == ConnectionType.Value:
                return $"value_output_{connectionViewModel.Slot}";

            case ConnectionSide.Input when ConnectionType == ConnectionType.Flow:
                return $"flow_input_{connectionViewModel.Slot}";

            case ConnectionSide.Input when ConnectionType == ConnectionType.Value:
                return $"value_input_{connectionViewModel.Slot}";

            default:
                return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class NodeItemsControlDataTemplateSelector : DataTemplateSelector
{
    public required DataTemplate? NodeTemplate { get; set; }
    public required DataTemplate? CastNodeTemplate { get; set; }
    public required DataTemplate? ValueInputNodeTemplate { get; set; }
    public required DataTemplate? BoolValueInputNodeTemplate { get; set; }
    public required DataTemplate? ButtonInputNodeTemplate { get; set; }
    public required DataTemplate? ValueOnlyNodeTemplate { get; set; }
    public required DataTemplate? NodeGroupTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is null) return null;

        if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(CastNode<,>)) return CastNodeTemplate;

        if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(ValueNode<>))
        {
            if (item.GetType().GenericTypeArguments[0] == typeof(bool)) return BoolValueInputNodeTemplate;

            return ValueInputNodeTemplate;
        }

        if (item is ButtonInputNode) return ButtonInputNodeTemplate;

        if (item is Node node &&
            node.IsValueNode(ConnectionSide.Input | ConnectionSide.Output) &&
            !node.IsFlowNode(ConnectionSide.Input | ConnectionSide.Output) &&
            node.NodeScape.GetMetadata(node).ValueInputNames.All(string.IsNullOrEmpty) &&
            node.NodeScape.GetMetadata(node).ValueOutputNames.All(string.IsNullOrEmpty)) return ValueOnlyNodeTemplate;

        if (item is Node) return NodeTemplate;
        if (item is NodeGroupViewModel) return NodeGroupTemplate;

        return null;
    }
}