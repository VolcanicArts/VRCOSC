// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCOSC.App.Nodes;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes.ViewModels;

public class NodeViewModel : GridGraphElementViewModel
{
    public NodeViewModel(INode node)
    {
        Node = node;
        populate(node);
        base.SetPosition(new Point(node.Metadata.Position.X, node.Metadata.Position.Y));
    }

    private void populate(INode node)
    {
        FlowInputVms = populateForPoint(node, ConnectionPoint.FlowInput).ToArray();
        FlowOutputVms = populateForPoint(node, ConnectionPoint.FlowOutput).ToArray();
        ValueInputVms = populateForPoint(node, ConnectionPoint.ValueInput).ToArray();
        ValueOutputVms = populateForPoint(node, ConnectionPoint.ValueOutput).ToArray();
    }

    private IEnumerable<ObservableObject> populateForPoint(INode node, ConnectionPoint point)
    {
        var points = node.Metadata.Elements[point];

        var isSourceNode = node.Metadata.Shared.IsSourceNode;
        var isDriveNode = node.Metadata.Shared.IsDriveNode;

        var valueOutputRenderName = !isSourceNode;
        var valueInputRenderName = !isDriveNode;
        bool? valueInputRenderInlineOverride = isDriveNode ? false : null;

        return point switch
        {
            ConnectionPoint.FlowOutput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeFlowOutputListViewModel((IFlowOutputList)e.Instance) : new NodeFlowOutputViewModel((IFlowOutput)e.Instance)),
            ConnectionPoint.FlowInput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeFlowInputListViewModel((IFlowInputList)e.Instance) : new NodeFlowInputViewModel((IFlowInput)e.Instance)),
            ConnectionPoint.ValueOutput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeValueOutputListViewModel((IValueOutputList)e.Instance) : new NodeValueOutputViewModel((IValueOutput)e.Instance, renderName: valueOutputRenderName)),
            ConnectionPoint.ValueInput => points.Select<INodeElementMetadata, ObservableObject>(e => e.Shared.IsList ? new NodeValueInputListViewModel((IValueInputList)e.Instance) : new NodeValueInputViewModel((IValueInput)e.Instance, renderName: valueInputRenderName, renderInlineOverride: valueInputRenderInlineOverride)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public INode Node { get; }

    public string DisplayName => Node.DisplayName;
    public string GenericTypesToString => string.Join(", ", Node.Metadata.Shared.GenericTypes.Select(arg => arg.GetFriendlyName()));

    public FrameworkElement SnappingControl { get; set; } = null!;

    public List<FrameworkElement>[] FlowInputControls { get; set; } = [];
    public List<FrameworkElement>[] FlowOutputControls { get; set; } = [];
    public List<FrameworkElement>[] ValueInputControls { get; set; } = [];
    public List<FrameworkElement>[] ValueOutputControls { get; set; } = [];

    public ObservableObject[] FlowInputVms { get; private set; } = [];
    public ObservableObject[] FlowOutputVms { get; private set; } = [];
    public ObservableObject[] ValueInputVms { get; private set; } = [];
    public ObservableObject[] ValueOutputVms { get; private set; } = [];

    public override void SetPosition(Point newPosition)
    {
        Node.Metadata.Position = new Vector2((float)newPosition.X, (float)newPosition.Y);
        base.SetPosition(newPosition);
    }

    public void NotifyProperty(string propertyName) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
}