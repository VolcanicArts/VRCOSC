// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Nodes.Serialisation.V2;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation.V1;

public class NodePresetSerialiserV1 : ProfiledSerialiser<NodePreset, SerialisableNodePresetV1>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "presets");
    protected override string FileName => $"{Reference.Id}.json";
    protected override Formatting Format => Formatting.None;

    public NodePresetSerialiserV1(Storage storage, NodePreset reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodePresetV1 data)
    {
        Reference.Id = data.Id;
        Reference.Name.Value = data.Name;

        // Migration
        Reference.Structure.Nodes = data.Nodes.Select(v1 => new SerialisableNode(v1)).ToList();
        Reference.Structure.Connections = data.Connections.Select(v1 => new SerialisableConnection(v1, Reference.Structure.Nodes)).ToList();
        Reference.Structure.Groups = data.Groups.Select(v1 => new SerialisableGroup(v1)).ToList();
        Reference.Structure.Variables = data.Variables.Select(v1 => new SerialisableGraphVariable(v1)).ToList();

        return true;
    }
}