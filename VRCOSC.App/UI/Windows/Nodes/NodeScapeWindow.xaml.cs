// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Nodes.Types.Operators;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Nodes;

namespace VRCOSC.App.UI.Windows.Nodes;

public sealed partial class NodeScapeWindow : IManagedWindow
{
    public NodeScape NodeScape { get; }

    public NodeScapeWindow(NodeScape nodeScape)
    {
        NodeScape = nodeScape;
        InitializeComponent();
        DataContext = this;

        NodeScape.AddNode<EqualsNode<string>>();

        AddChild(new NodeScapeView(NodeScape));
    }

    public object GetComparer() => NodeScape;
}