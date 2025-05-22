// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FontAwesome6;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Types.Base;
using VRCOSC.App.Nodes.Types.Flow;
using VRCOSC.App.Nodes.Types.Flow.Impulse;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.Nodes.Types.Values;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes;

public record ConnectionViewModel(Node Node, int Slot, string Title, Type? Type);

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

                return new ConnectionViewModel[] { new(node, 0, string.Empty, null) };
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!node.Metadata.IsFlowOutput) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, node.Metadata.FlowCount).Select(i => new ConnectionViewModel(node, i, node.Metadata.FlowOutputs[i].Name, null)).ToList();
            }
        }

        if (ConnectionType == ConnectionType.Value && node.Metadata.IsValue)
        {
            if (ConnectionSide == ConnectionSide.Input)
            {
                if (!node.Metadata.IsValueInput) return Array.Empty<ConnectionViewModel>();

                var length = node.Metadata.InputsCount;
                var points = Enumerable.Range(0, length).Select(i => new ConnectionViewModel(node, i, node.Metadata.Inputs[i].Name, node.Metadata.Inputs[i].Type)).ToList();

                if (node.Metadata.ValueInputHasVariableSize)
                {
                    points.Remove(points.Last());

                    points.AddRange(Enumerable.Range(0, node.VariableSize.ValueInputSize).Select(i =>
                        new ConnectionViewModel(node, node.Metadata.InputsCount - 1 + i, i == 0 ? node.Metadata.Inputs.Last().Name : string.Empty, node.Metadata.Inputs.Last().Type)));
                }

                return points;
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!node.Metadata.IsValueOutput) return Array.Empty<ConnectionViewModel>();

                var length = node.Metadata.OutputsCount;
                var points = Enumerable.Range(0, length).Select(i => new ConnectionViewModel(node, i, node.Metadata.Outputs[i].Name, node.Metadata.Outputs[i].Type)).ToList();

                if (node.Metadata.ValueOutputHasVariableSize)
                {
                    points.Remove(points.Last());

                    points.AddRange(Enumerable.Range(0, node.VariableSize.ValueOutputSize).Select(i =>
                        new ConnectionViewModel(node, node.Metadata.OutputsCount - 1 + i, i == 0 ? node.Metadata.Outputs.Last().Name : string.Empty, node.Metadata.Outputs.Last().Type)));
                }

                return points;
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

public class ConnectionAreaVisibilityConverter : IValueConverter
{
    public bool CheckFlow { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return Visibility.Collapsed;

        if (CheckFlow)
        {
            return node.Metadata.IsFlow ? Visibility.Visible : Visibility.Collapsed;
        }

        return node.Metadata.IsValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class ValueVariableSizeControlVisibilityConverter : IValueConverter
{
    public bool CheckOutput { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return Visibility.Collapsed;

        if (CheckOutput && node.Metadata.IsValueOutput)
        {
            return node.Metadata.ValueOutputHasVariableSize ? Visibility.Visible : Visibility.Collapsed;
        }

        if (!CheckOutput && node.Metadata.IsValueInput)
        {
            return node.Metadata.ValueInputHasVariableSize ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class NodeIconToTitleVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EFontAwesomeIcon icon) return Visibility.Collapsed;

        return icon == EFontAwesomeIcon.None ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class NodeItemsControlDataTemplateSelector : DataTemplateSelector
{
    public required DataTemplate? NodeTemplate { get; set; }
    public required DataTemplate? RelayNodeTemplate { get; set; }
    public required DataTemplate? ValueInputNodeTemplate { get; set; }
    public required DataTemplate? BoolValueInputNodeTemplate { get; set; }
    public required DataTemplate? EnumValueInputNodeTemplate { get; set; }
    public required DataTemplate? KeybindValueInputNodeTemplate { get; set; }
    public required DataTemplate? ButtonInputNodeTemplate { get; set; }
    public required DataTemplate? ImpulseSendNodeTemplate { get; set; }
    public required DataTemplate? ImpulseReceiveNodeTemplate { get; set; }
    public required DataTemplate? ValueOnlyNodeTemplate { get; set; }
    public required DataTemplate? AnyParameterReceivedNodeTemplate { get; set; }
    public required DataTemplate? NodeGroupTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is null) return null;

        if (item is ReceiveImpulseNode) return ImpulseReceiveNodeTemplate;
        if (item is SendImpulseNode) return ImpulseSendNodeTemplate;

        if (item.GetType().IsGenericType &&
            (item.GetType().GetGenericTypeDefinition() == typeof(CastNode<,>) ||
             item.GetType().GetGenericTypeDefinition() == typeof(ValueRelayNode<>))) return RelayNodeTemplate;

        if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(ValueNode<>))
        {
            if (item.GetType().GenericTypeArguments[0] == typeof(bool)) return BoolValueInputNodeTemplate;
            if (item.GetType().GenericTypeArguments[0] == typeof(Keybind)) return KeybindValueInputNodeTemplate;
            if (item.GetType().GenericTypeArguments[0].IsAssignableTo(typeof(Enum))) return EnumValueInputNodeTemplate;

            return ValueInputNodeTemplate;
        }

        if (item is ButtonInputNode) return ButtonInputNodeTemplate;

        if (item is Node node && (node.GetType().HasCustomAttribute<NodeCollapsedAttribute>() || node.Metadata.Icon != EFontAwesomeIcon.None)) return ValueOnlyNodeTemplate;

        if (item is Node) return NodeTemplate;
        if (item is NodeGroupViewModel) return NodeGroupTemplate;

        return null;
    }
}