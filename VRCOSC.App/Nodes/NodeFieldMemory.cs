// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeFieldMemory
{
    private int scope;
    private readonly List<NodeFieldMemoryEntry> entries = [];

    public void Reset()
    {
        scope = 0;
        entries.Clear();
    }

    public void CreateEntry(Node node)
    {
        var metadata = node.Metadata;

        var entryArray = new IRef[metadata.OutputsCount];

        for (var i = 0; i < metadata.OutputsCount; i++)
        {
            var outputMetadata = metadata.Outputs[i];

            if (metadata.OutputHasVariableSize && i == metadata.OutputsCount - 1)
            {
                var arrSize = metadata.OutputVariableSizeActual;
                var elementType = outputMetadata.Type.GetElementType()!;
                var arr = Array.CreateInstance(elementType, arrSize);
                var defaultValue = elementType.CreateDefault();

                for (var j = 0; j < arrSize; j++)
                {
                    arr.SetValue(defaultValue, i);
                }

                entryArray[i] = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(outputMetadata.Type), args: [arr])!;
            }
            else
            {
                var defaultValue = outputMetadata.Type.CreateDefault();
                entryArray[i] = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(outputMetadata.Type), args: [defaultValue])!;
            }
        }

        entries.Add(new NodeFieldMemoryEntry(node.Id, entryArray, scope));
    }

    public NodeFieldMemoryEntry Read(Guid nodeId)
    {
        return entries.First(entry => entry.NodeId == nodeId);
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

public class NodeFieldMemoryEntry
{
    public readonly Guid NodeId;
    public IRef[] Values;
    public readonly int Scope;

    public NodeFieldMemoryEntry(Guid nodeId, IRef[] values, int scope)
    {
        NodeId = nodeId;
        Values = values;
        Scope = scope;
    }
}

public interface IRef
{
    public object? GetValue();
}

public class Ref<T> : IRef
{
    public T Value;

    public Ref(T startValue)
    {
        Value = startValue;
    }

    public object? GetValue() => Value;
}