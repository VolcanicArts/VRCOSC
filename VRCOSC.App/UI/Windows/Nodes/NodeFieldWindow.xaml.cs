// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Nodes.Types;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Nodes;

namespace VRCOSC.App.UI.Windows.Nodes;

public sealed partial class NodeFieldWindow : IManagedWindow
{
    public NodeField NodeField { get; }

    public NodeFieldWindow(NodeField nodeField)
    {
        var triggerNode = new TriggerNode(nodeField);
        nodeField.AddNode(triggerNode);

        NodeField = nodeField;
        InitializeComponent();
        DataContext = this;

        AddChild(new NodeCanvas(NodeField));
    }

    public object GetComparer() => NodeField;
}