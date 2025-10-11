// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation;

public class NodePresetSerialiser : ProfiledSerialiser<NodePreset, SerialisableNodePreset>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "presets");
    protected override string FileName => $"{Reference.Id}.json";
    protected override Formatting Format => Formatting.None;

    public NodePresetSerialiser(Storage storage, NodePreset reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodePreset data)
    {
        Reference.Name.Value = data.Name;
        Reference.Nodes = data.Nodes;
        Reference.Connections = data.Connections;
        Reference.Groups = data.Groups;
        return false;
    }
}