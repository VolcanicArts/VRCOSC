// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Reflection;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Nodes.Types.Converters;
using VRCOSC.App.SDK.Nodes.Types.Debug;
using VRCOSC.App.SDK.Nodes.Types.Flow;
using VRCOSC.App.SDK.Nodes.Types.Parameters;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Nodes;

namespace VRCOSC.App.UI.Windows.Nodes;

public sealed partial class NodeScapeWindow : IManagedWindow
{
    public NodeScape NodeScape { get; }

    public NodeScapeWindow(NodeScape nodeScape)
    {
        foreach (var type in Assembly.GetCallingAssembly().GetExportedTypes().Where(type => type.IsAssignableTo(typeof(Node)) && !type.IsAbstract))
        {
            // lol
            var method = nodeScape.GetType().GetMethod("RegisterNode")!;
            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(nodeScape, null);
        }

        nodeScape.AddNode<ForNode>();
        nodeScape.AddNode<ForEachNode>();
        nodeScape.AddNode<EnumerableLengthNode>();
        nodeScape.AddNode<ElementAtNode>();
        nodeScape.AddNode<ListOutputTestNode>();
        nodeScape.AddNode<PrintNode>();
        nodeScape.AddNode<PassthroughListTestNode>();
        nodeScape.AddNode<OnRegisteredParameterReceived>();
        nodeScape.AddNode<IfWithStateNode>();

        NodeScape = nodeScape;
        InitializeComponent();
        DataContext = this;

        AddChild(new NodeScapeView(NodeScape));
    }

    public object GetComparer() => NodeScape;
}