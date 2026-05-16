// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.UI.Views.Nodes.ViewModels;

namespace VRCOSC.App.UI.Views.Nodes.Converters;

public class ConnectionItemsSourceConverter : IValueConverter
{
    public required ConnectionPoint ConnectionPoint { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not NodeViewModel nodeViewModel) return null;

        var node = nodeViewModel.Node;
        var points = node.Metadata.Elements[ConnectionPoint];

        var isSourceNode = node.Metadata.IsSourceNode;

        return ConnectionPoint switch
        {
            ConnectionPoint.FlowOutput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeFlowOutputListViewModel((IFlowOutputList)e.Instance) : new NodeFlowOutputViewModel((IFlowOutput)e.Instance)),
            ConnectionPoint.FlowInput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeFlowInputListViewModel((IFlowInputList)e.Instance) : new NodeFlowInputViewModel((IFlowInput)e.Instance)),
            ConnectionPoint.ValueOutput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeValueOutputListViewModel((IValueOutputList)e.Instance) : new NodeValueOutputViewModel((IValueOutput)e.Instance, renderName: !isSourceNode)),
            ConnectionPoint.ValueInput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeValueInputListViewModel((IValueInputList)e.Instance) : new NodeValueInputViewModel((IValueInput)e.Instance)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}