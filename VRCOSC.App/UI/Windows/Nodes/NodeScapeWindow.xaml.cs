// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Nodes.Types.Converters;
using VRCOSC.App.SDK.Nodes.Types.Debug;
using VRCOSC.App.SDK.Nodes.Types.Flow;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Nodes;

namespace VRCOSC.App.UI.Windows.Nodes;

public sealed partial class NodeScapeWindow : IManagedWindow
{
    public NodeScape NodeScape { get; }

    public NodeScapeWindow(NodeScape nodeScape)
    {
        nodeScape.AddNode<ForNode>();
        nodeScape.AddNode<ForEachNode<string>>();
        nodeScape.AddNode<EnumerableCountNode<string>>();
        nodeScape.AddNode<ElementAtNode<string>>();
        nodeScape.AddNode<ListOutputTestNode>();
        nodeScape.AddNode<LogNode>();
        nodeScape.AddNode<PassthroughListTestNode<string>>();
        nodeScape.AddNode<IfWithStateNode>();
        nodeScape.AddNode<ButtonInputNode>();

        NodeScape = nodeScape;
        InitializeComponent();
        DataContext = this;

        AddChild(new NodeScapeView(NodeScape));
    }

    public object GetComparer() => NodeScape;
}