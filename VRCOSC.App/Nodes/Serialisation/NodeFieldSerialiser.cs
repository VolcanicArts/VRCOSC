// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Serialisation;

public class NodeFieldSerialiser : ProfiledSerialiser<NodeField, SerialisableNodeField>
{
    protected override string Directory => Path.Join(base.Directory, "nodes");
    protected override string FileName => $"{Reference.Id}.json";

    public NodeFieldSerialiser(Storage storage, NodeField reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableNodeField data)
    {
        // TODO: ZIndex is not saved as the nodes are saved in that order, use the import as a management opportunity to collapse all the ZIndexes
        return false;
    }
}