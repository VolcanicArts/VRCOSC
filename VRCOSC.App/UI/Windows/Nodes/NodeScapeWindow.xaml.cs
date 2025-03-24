// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Nodes.Types;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Nodes;

namespace VRCOSC.App.UI.Windows.Nodes;

public sealed partial class NodeScapeWindow : IManagedWindow
{
    public NodeScape NodeScape { get; }

    public NodeScapeWindow(NodeScape nodeScape)
    {
        var stringTextNode1 = new StringTextNode();
        var stringTextNode2 = new StringTextNode();

        nodeScape.AddNode(new ParameterReceivedTriggerNode());
        nodeScape.AddNode(new BranchNode());
        nodeScape.AddNode(new PrintNode());
        nodeScape.AddNode(new PrintNode());
        nodeScape.AddNode(stringTextNode1);
        nodeScape.AddNode(stringTextNode2);
        nodeScape.AddNode(new IntTextNode());
        nodeScape.AddNode(new IntTextNode());
        nodeScape.AddNode(new IsEqualNode());
        nodeScape.AddNode(new WhileNode());
        nodeScape.AddNode(new ForNode());
        nodeScape.AddNode(new FlowSpitNode());

        var group = nodeScape.AddGroup();
        group.Nodes.Add(stringTextNode1.Id);
        group.Nodes.Add(stringTextNode2.Id);

        NodeScape = nodeScape;
        InitializeComponent();
        DataContext = this;

        AddChild(new NodeScapeView(NodeScape));
    }

    public object GetComparer() => NodeScape;
}