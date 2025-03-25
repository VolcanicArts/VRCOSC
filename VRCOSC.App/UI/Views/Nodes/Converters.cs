// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.UI.Views.Nodes;

public record ConnectionViewModel(int Slot, string Title);

public class ConnectionAmountConverter : IValueConverter
{
    public ConnectionSide ConnectionSide { get; set; }
    public ConnectionType ConnectionType { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Node node) return null;

        if (ConnectionType == ConnectionType.Flow && node.IsFlowNode(ConnectionSide.Input | ConnectionSide.Output))
        {
            if (ConnectionSide == ConnectionSide.Input && node.IsFlowNode(ConnectionSide.Input))
            {
                return !node.GetFlowInputAttribute().IsTrigger ? Enumerable.Range(0, 1).Select(i => new ConnectionViewModel(i, string.Empty)) : Array.Empty<ConnectionViewModel>();
            }

            if (ConnectionSide == ConnectionSide.Output && node.IsFlowNode(ConnectionSide.Output))
            {
                var attribute = node.GetFlowOutputAttribute();
                return attribute.Count > 0 ? Enumerable.Range(0, attribute.Count).Select(i => new ConnectionViewModel(i, attribute.Titles[i])) : Array.Empty<ConnectionViewModel>();
            }
        }

        if (ConnectionType == ConnectionType.Value && node.IsValueNode(ConnectionSide.Input | ConnectionSide.Output))
        {
            if (ConnectionSide == ConnectionSide.Input && node.IsValueNode(ConnectionSide.Input))
            {
                var attribute = node.GetValueInputAttribute();
                return attribute.Count > 0 ? Enumerable.Range(0, attribute.Count).Select(i => new ConnectionViewModel(i, attribute.Titles[i])) : Array.Empty<ConnectionViewModel>();
            }

            if (ConnectionSide == ConnectionSide.Output && node.IsValueNode(ConnectionSide.Output))
            {
                var attribute = node.GetValueOutputAttribute();
                return attribute.Count > 0 ? Enumerable.Range(0, attribute.Count).Select(i => new ConnectionViewModel(i, attribute.Titles[i])) : Array.Empty<ConnectionViewModel>();
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
    public required DataTemplate? NodeGroupTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is Node) return NodeTemplate;
        if (item is NodeGroupViewModel) return NodeGroupTemplate;

        return null;
    }
}