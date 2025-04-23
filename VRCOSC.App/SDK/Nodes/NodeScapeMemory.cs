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

    public void Write(Guid nodeId, object?[] value)
    {
        entries.RemoveIf(entry => entry.NodeId == nodeId);
        entries.Add(new NodeScapeMemoryEntry(nodeId, value, scope));
    }

    public object? Read(Guid nodeId, int slot)
    {
        return entries.SingleOrDefault(entry => entry.NodeId == nodeId)?.Value[slot];
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
}

public record NodeScapeMemoryEntry(Guid NodeId, object?[] Value, int Scope);