// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.App.Nodes.Types;

namespace VRCOSC.App.Nodes;

public class GraphChanges
{
    public List<INode> RemovedNodes { get; } = [];
    public List<INode> AddedNodes { get; } = [];
    public List<IConnection> RemovedConnections { get; } = [];
    public List<IConnection> AddedConnections { get; } = [];
    public List<NodeGroup> RemovedGroups { get; } = [];
    public List<NodeGroup> AddedGroups { get; } = [];
}