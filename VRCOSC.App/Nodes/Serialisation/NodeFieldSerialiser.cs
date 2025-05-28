// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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
        Reference.Name = data.Name;

        data.Nodes.ForEach(sN =>
        {
            var declaration = TypeResolver.Parse(sN.Type);
            var baseType = TypeResolver.ResolveType(declaration.ClassName)!;
            var generics = declaration.Generics;

            Type nodeType = baseType;

            if (!string.IsNullOrEmpty(generics))
            {
                if (TypeResolver.TryConstructGenericType(generics, baseType, out var genericNodeType))
                {
                    nodeType = genericNodeType;
                }
            }

            var node = Reference.AddNode(sN.Id, nodeType);

            node.Position = new ObservableVector2(sN.Position.X, sN.Position.Y);
            node.ZIndex = sN.ZIndex;
        });

        data.Connections.ForEach(sC =>
        {
            if (sC.Type == ConnectionType.Flow)
                Reference.CreateFlowConnection(sC.OutputNodeId, sC.OutputNodeSlot, sC.InputNodeId);

            if (sC.Type == ConnectionType.Value)
                Reference.CreateValueConnection(sC.OutputNodeId, sC.OutputNodeSlot, sC.InputNodeId, sC.InputNodeSlot);
        });

        data.Groups.ForEach(sG =>
        {
            var group = Reference.AddGroup(sG.Id);
            group.Title = sG.Title;
            group.Nodes.AddRange(sG.Nodes);
        });

        return false;
    }
}