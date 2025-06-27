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
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Nodes.Types.Flow;
using VRCOSC.App.Nodes.Types.Inputs;
using VRCOSC.App.Nodes.Types.Utility;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes;

public record ConnectionViewModel(NodeGraphItem NodeGraphItem, int Slot, string Title, Type? Type);

public class ConnectionAmountConverter : IValueConverter
{
    public ConnectionSide ConnectionSide { get; set; }
    public ConnectionType ConnectionType { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not NodeGraphItem item) return null;

        if (ConnectionType == ConnectionType.Flow && item.Node.Metadata.IsFlow)
        {
            if (ConnectionSide == ConnectionSide.Input)
            {
                if (item.Node.Metadata.IsTrigger) return Array.Empty<ConnectionViewModel>();
                if (!item.Node.Metadata.IsFlowInput) return Array.Empty<ConnectionViewModel>();

                return new ConnectionViewModel[] { new(item, 0, string.Empty, null) };
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!item.Node.Metadata.IsFlowOutput) return Array.Empty<ConnectionViewModel>();

                return Enumerable.Range(0, item.Node.Metadata.FlowCount).Select(i => new ConnectionViewModel(item, i, item.Node.Metadata.FlowOutputs[i].Name, null)).ToList();
            }
        }

        if (ConnectionType == ConnectionType.Value && item.Node.Metadata.IsValue)
        {
            if (ConnectionSide == ConnectionSide.Input)
            {
                if (!item.Node.Metadata.IsValueInput) return Array.Empty<ConnectionViewModel>();

                var length = item.Node.Metadata.InputsCount;
                var points = Enumerable.Range(0, length).Select(i => new ConnectionViewModel(item, i, item.Node.Metadata.Inputs[i].Name, item.Node.Metadata.Inputs[i].Type)).ToList();

                if (item.Node.Metadata.ValueInputHasVariableSize)
                {
                    points.Remove(points.Last());

                    points.AddRange(Enumerable.Range(0, item.Node.VariableSize.ValueInputSize).Select(i =>
                        new ConnectionViewModel(item, item.Node.Metadata.InputsCount - 1 + i, i == 0 ? item.Node.Metadata.Inputs.Last().Name : string.Empty, item.Node.Metadata.Inputs.Last().Type)));
                }

                return points;
            }

            if (ConnectionSide == ConnectionSide.Output)
            {
                if (!item.Node.Metadata.IsValueOutput) return Array.Empty<ConnectionViewModel>();

                var length = item.Node.Metadata.OutputsCount;
                var points = Enumerable.Range(0, length).Select(i => new ConnectionViewModel(item, i, item.Node.Metadata.Outputs[i].Name, item.Node.Metadata.Outputs[i].Type)).ToList();

                if (item.Node.Metadata.ValueOutputHasVariableSize)
                {
                    points.Remove(points.Last());

                    points.AddRange(Enumerable.Range(0, item.Node.VariableSize.ValueOutputSize).Select(i =>
                        new ConnectionViewModel(item, item.Node.Metadata.OutputsCount - 1 + i, i == 0 ? item.Node.Metadata.Outputs.Last().Name : string.Empty, item.Node.Metadata.Outputs.Last().Type)));
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

        // TODO: Convert this to be a struct of ConnectionType and Slot
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
        if (value is not NodeGraphItem item) return Visibility.Collapsed;

        if (CheckFlow)
        {
            return item.Node.Metadata.IsFlow ? Visibility.Visible : Visibility.Collapsed;
        }

        return item.Node.Metadata.IsValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}

public class ValueVariableSizeControlVisibilityConverter : IValueConverter
{
    public bool CheckOutput { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not NodeGraphItem item) return Visibility.Collapsed;

        if (CheckOutput && item.Node.Metadata.IsValueOutput)
        {
            return item.Node.Metadata.ValueOutputHasVariableSize ? Visibility.Visible : Visibility.Collapsed;
        }

        if (!CheckOutput && item.Node.Metadata.IsValueInput)
        {
            return item.Node.Metadata.ValueInputHasVariableSize ? Visibility.Visible : Visibility.Collapsed;
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

public class GraphItemsDataTemplateSelector : DataTemplateSelector
{
    public required DataTemplate? NodeTemplate { get; set; }
    public required DataTemplate? RelayNodeTemplate { get; set; }
    public required DataTemplate? RichTextBoxNodeTemplate { get; set; }
    public required DataTemplate? TextBoxValueOutputOnlyNodeTemplate { get; set; }
    public required DataTemplate? ToggleValueOutputOnlyNodeTemplate { get; set; }
    public required DataTemplate? EnumValueOutputOnlyNodeTemplate { get; set; }
    public required DataTemplate? KeybindValueOutputOnlyNodeTemplate { get; set; }
    public required DataTemplate? ButtonNodeTemplate { get; set; }
    public required DataTemplate? CollapsedNodeTemplate { get; set; }
    public required DataTemplate? CollapsedOutputOnlyNodeTemplate { get; set; }
    public required DataTemplate? NodeGroupTemplate { get; set; }
    public required DataTemplate? TextBoxSourceNodeTemplate { get; set; }
    public required DataTemplate? TextBoxDriveNodeTemplate { get; set; }
    public required DataTemplate? NodeWithTextBoxTemplate { get; set; }
    public required DataTemplate? DisplayNodeTemplate { get; set; }
    public required DataTemplate? PassthroughDisplayNodeTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is NodeGroupGraphItem nodeGroupGraphItem)
        {
            return NodeGroupTemplate;
        }

        if (item is NodeGraphItem nodeGraphItem)
        {
            var node = nodeGraphItem.Node;
            var type = node.GetType();

            var metadata = node.Metadata;

            if (node is ButtonNode) return ButtonNodeTemplate;
            if (node is RichTextNode) return RichTextBoxNodeTemplate;

            if (type.IsGenericType)
            {
                var genDef = type.GetGenericTypeDefinition();

                if (genDef == typeof(DisplayNode<>)) return DisplayNodeTemplate;
                if (genDef == typeof(PassthroughDisplayNode<>)) return PassthroughDisplayNodeTemplate;
                if (genDef == typeof(CastNode<,>)) return RelayNodeTemplate;
                if (genDef == typeof(RelayNode<>)) return RelayNodeTemplate;

                if (genDef == typeof(ValueNode<>))
                {
                    var valueType = type.GenericTypeArguments[0];

                    if (valueType == typeof(bool)) return ToggleValueOutputOnlyNodeTemplate;
                    if (valueType == typeof(Keybind)) return KeybindValueOutputOnlyNodeTemplate;
                    if (valueType.IsAssignableTo(typeof(Enum))) return EnumValueOutputOnlyNodeTemplate;

                    return TextBoxValueOutputOnlyNodeTemplate;
                }
            }

            if (type.IsAssignableTo(typeof(IHasTextProperty)))
            {
                if (metadata is { IsFlow: false, IsValueInput: false, IsValueOutput: true, OutputsCount: 1 }) return TextBoxSourceNodeTemplate;
                if (metadata is { IsFlow: false, IsValueInput: true, IsValueOutput: false, InputsCount: 1 }) return TextBoxDriveNodeTemplate;

                return NodeWithTextBoxTemplate;
            }

            if (type.HasCustomAttribute<NodeCollapsedAttribute>() || metadata.Icon != EFontAwesomeIcon.None)
            {
                if (metadata is { IsValueInput: false, IsValueOutput: true }) return CollapsedOutputOnlyNodeTemplate;

                return CollapsedNodeTemplate;
            }

            return NodeTemplate;
        }

        return null;
    }
}