// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Nodes.Types.Base;
using VRCOSC.App.SDK.Nodes.Types.Converters;
using VRCOSC.App.SDK.Nodes.Types.Inputs;

namespace VRCOSC.App.UI.Views.Nodes;

public record ConnectionViewModel(int Slot, string Title, Type? Type);

public class ConnectionAmountConverter : IValueConverter
{
    public ConnectionSide ConnectionSide { get; set; }
    public ConnectionType ConnectionType { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return null;

        if (ConnectionType == ConnectionType.Flow && node.Metadata.IsFlow)
        {
            if (ConnectionSide == ConnectionSide.Input)
            {
                if (node.Metadata.IsTrigger) return Array.Empty<ConnectionViewModel>();
                if (!node.Metadata.IsFlowInput) return Array.Empty<ConnectionViewModel>();

                return new ConnectionViewModel[] { new(0, "*", null) };
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!node.Metadata.IsFlowOutput) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, node.Metadata.FlowOutputs.Length).Select(i => new ConnectionViewModel(i, node.Metadata.FlowOutputs[i].Name, null));
            }
        }

        if (ConnectionType == ConnectionType.Value && node.Metadata.IsValue)
        {
            if (ConnectionSide == ConnectionSide.Input)
            {
                if (!node.Metadata.IsValueInput) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, node.Metadata.Process.Inputs.Length).Select(i => new ConnectionViewModel(i, node.Metadata.Process.Inputs[i].Name, node.Metadata.Process.Inputs[i].Type));
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!node.Metadata.IsValueOutput) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, node.Metadata.Process.Outputs.Length).Select(i => new ConnectionViewModel(i, node.Metadata.Process.Outputs[i].Name, node.Metadata.Process.Outputs[i].Type));
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
    public required DataTemplate? EnumValueInputNodeTemplate { get; set; }
    public required DataTemplate? ButtonInputNodeTemplate { get; set; }
    public required DataTemplate? ImpulseTriggerNodeTemplate { get; set; }
    public required DataTemplate? ImpulseReceiverNodeTemplate { get; set; }
    public required DataTemplate? ValueOnlyNodeTemplate { get; set; }
    public required DataTemplate? NodeGroupTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is null) return null;

        if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(CastNode<,>)) return CastNodeTemplate;

        if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(ValueNode<>))
        {
            if (item.GetType().GenericTypeArguments[0] == typeof(bool)) return BoolValueInputNodeTemplate;
            if (item.GetType().GenericTypeArguments[0].IsAssignableTo(typeof(Enum))) return EnumValueInputNodeTemplate;

            return ValueInputNodeTemplate;
        }

        if (item is ButtonInputNode) return ButtonInputNodeTemplate;

        if (item is Node { Metadata: { IsValue: true, IsFlow: false } } node &&
            node.Metadata.Process.Inputs.All(input => string.IsNullOrEmpty(input.Name)) &&
            node.Metadata.Process.Outputs.All(output => string.IsNullOrEmpty(output.Name))) return ValueOnlyNodeTemplate;

        if (item is Node) return NodeTemplate;
        if (item is NodeGroupViewModel) return NodeGroupTemplate;

        return null;
    }
}