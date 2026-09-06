// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Nodes.Serialisation.V2;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation.V1;

public class NodeGraphSerialiserV1 : ProfiledSerialiser<NodeGraph, SerialisableNodeGraphV1>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "graphs");
    protected override string FileName => $"{Reference.Id}.json";
    protected override Formatting Format => Formatting.None;

    public NodeGraphSerialiserV1(Storage storage, NodeGraph reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodeGraphV1 data)
    {
        Reference.Id = Reference.FromImport ? Guid.NewGuid() : data.Id;
        Reference.Name.Value = data.Name;
        Reference.Enabled.Value = Reference.FromImport || data.Enabled;

        // Migration
        var nodes = data.Nodes.Select(v1 => new SerialisableNode(v1)).ToList();
        var connections = data.Connections.Select(v1 => new SerialisableConnection(v1, nodes)).ToList();
        var groups = data.Groups.Select(v1 => new SerialisableGroup(v1)).ToList();
        var variables = data.Variables.Select(v1 => new SerialisableGraphVariable(v1)).ToList();

        var baseElements = new SerialisableNodeGraphBase
        {
            Nodes = nodes,
            Connections = connections,
            Groups = groups,
            Variables = variables
        };

        NodeGraphBaseHelper.Deserialise(baseElements, Reference, Reference.FromImport);
        return true;
    }
}