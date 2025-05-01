// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Nodes;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Windows.Nodes;

public partial class NodeGroupMetadataWindow : IManagedWindow
{
    public NodeGroup NodeGroup { get; }

    public NodeGroupMetadataWindow(NodeGroup nodeGroup)
    {
        InitializeComponent();
        NodeGroup = nodeGroup;
        DataContext = NodeGroup;
    }

    public object GetComparer() => NodeGroup;
}