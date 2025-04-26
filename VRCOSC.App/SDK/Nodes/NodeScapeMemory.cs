// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public class NodeScapeMemory
{
    private int scope;
    private readonly List<NodeScapeMemoryEntry> entries = [];

    public void Reset()
    {
        scope = 0;
        entries.Clear();
    }

    public void CreateEntries(Node node)
    {
        var metadata = node.Metadata;

        entries.Add(new NodeScapeMemoryEntry(node.Id, metadata.Outputs.Select(outputMetadata =>
        {
            var defaultValue = getDefault(outputMetadata.Type);
            return (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(outputMetadata.Type), args: [defaultValue])!;
        }).ToArray(), scope));
    }

    public NodeScapeMemoryEntry Read(Guid nodeId)
    {
        return entries.Single(entry => entry.NodeId == nodeId);
    }

    public bool HasEntry(Guid nodeId)
    {
        return entries.Exists(entry => entry.NodeId == nodeId);
    }

    public void Push() => scope++;

    public void Pop()
    {
        scope--;
        if (scope < 0) throw new Exception("Scope is now less than 0. You've popped too much!");

        entries.RemoveIf(entry => entry.Scope > scope);
    }

    private object? getDefault(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
}

public class NodeScapeMemoryEntry
{
    public readonly Guid NodeId;
    public IRef[] Values;
    public readonly int Scope;

    public NodeScapeMemoryEntry(Guid nodeId, IRef[] values, int scope)
    {
        NodeId = nodeId;
        Values = values;
        Scope = scope;
    }
}

public interface IRef;

public class Ref<T> : IRef
{
    public T Value;

    public Ref(T value)
    {
        Value = value;
    }
}