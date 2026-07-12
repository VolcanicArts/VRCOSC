// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation.V2;

public class NodeGraphSerialiser : ProfiledSerialiser<NodeGraph, SerialisableNodeGraph>
{
    protected override string Directory => Path.Join(base.Directory, "nodes", "graphs");
    protected override string FileName => $"{Reference.Id}.json";
    protected override Formatting Format => Formatting.None;

    public NodeGraphSerialiser(Storage storage, NodeGraph reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodeGraph data)
    {
        Reference.Id = Reference.FromImport ? Guid.NewGuid() : data.Id;
        Reference.Name.Value = data.Name;
        Reference.Enabled.Value = Reference.FromImport || data.Enabled;
        NodeGraphBaseHelper.Deserialise(data, Reference, Reference.FromImport);
        return false;
    }
}