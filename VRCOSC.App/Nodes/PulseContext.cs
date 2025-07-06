// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class PulseContext
{
    internal CancellationTokenSource Source { get; }
    public CancellationToken Token { get; }

    public bool IsCancelled => Token.IsCancellationRequested;

    internal readonly NodeGraph Graph;
    internal Node? CurrentNode { get; set; }

    private Stack<Dictionary<Guid, IRef[]>> memory { get; } = [];
    private List<Guid> hasMemory { get; } = [];
    private Dictionary<Guid, Dictionary<IStore, IRef>> stores { get; } = [];

    internal PulseContext(NodeGraph graph)
    {
        Graph = graph;

        Source = new CancellationTokenSource();
        Token = Source.Token;

        memory.Push(new Dictionary<Guid, IRef[]>());
    }

    internal bool HasMemory(Guid nodeId) => hasMemory.Contains(nodeId);

    internal async Task Execute(FlowCall call)
    {
        memory.Push(new Dictionary<Guid, IRef[]>());
        await processNext(call);
        memory.Pop();
    }

    internal Task Execute(FlowContinuation continuation)
    {
        return processNext(continuation);
    }

    internal Task<AvatarConfig?> GetCurrentAvatar() => AppManager.GetInstance().FindCurrentAvatar(Token);

    internal Player GetPlayer() => AppManager.GetInstance().VRChatClient.Player;

    internal async Task<VRChatParameter?> GetParameter<T>(string name)
    {
        var parameterDefinition = new ParameterDefinition(name, ParameterTypeFactory.CreateFrom<T>());
        var parameter = await AppManager.GetInstance().FindParameter(parameterDefinition, Token);

        return parameter;
    }

    private Task processNext(IFlow next)
    {
        Debug.Assert(CurrentNode is not null);

        var outputIndex = next.Index;
        var connection = Graph.FindConnectionFromFlowOutput(CurrentNode.Id, outputIndex);
        return connection is null ? Task.CompletedTask : Graph.ProcessNode(connection.InputNodeId, this);
    }

    private T readValue<T>(Guid nodeId, int index)
    {
        var metadata = Graph.Nodes[nodeId].Metadata;
        var hasVariableSize = metadata.ValueOutputHasVariableSize;

        for (var i = 0; i < memory.Count; i++)
        {
            var innerMemory = memory.ElementAt(i);
            if (!innerMemory.TryGetValue(nodeId, out var refs)) continue;

            if (hasVariableSize && index >= metadata.OutputsCount - 1)
            {
                var variableRef = (T[])refs[metadata.OutputsCount - 1].GetValue()!;
                var subIndex = index - (metadata.OutputsCount - 1);
                return variableRef[subIndex];
            }

            return (T)refs[index].GetValue()!;
        }

        throw new InvalidOperationException($"Could not read from {nodeId} at index {index}");
    }

    private void writeValue<T>(Guid nodeId, int index, T value)
    {
        ((Ref<T>)memory.Peek()[nodeId][index]).Value = value;
    }

    private void writeValueList<T>(Guid nodeId, int index, int listIndex, T value)
    {
        ((Ref<T[]>)memory.Peek()[nodeId][index]).Value[listIndex] = value;
    }

    internal T Read<T>(ValueInput<T> valueInput)
    {
        Debug.Assert(CurrentNode is not null);

        var inputIndex = valueInput.Index;
        var connection = Graph.FindConnectionFromValueInput(CurrentNode.Id, inputIndex);
        return connection is null ? valueInput.DefaultValue : readValue<T>(connection.OutputNodeId, connection.OutputSlot);
    }

    internal List<T> Read<T>(ValueInputList<T> valueInputList)
    {
        Debug.Assert(CurrentNode is not null);

        var inputIndex = valueInputList.Index;

        var list = new List<T>();

        for (var i = inputIndex; i < CurrentNode.VirtualValueInputCount(); i++)
        {
            var connection = Graph.FindConnectionFromValueInput(CurrentNode.Id, i);
            list.Add(connection is null ? default! : readValue<T>(connection.OutputNodeId, connection.OutputSlot));
        }

        return list;
    }

    internal void Write<T>(ValueOutput<T> valueOutput, T value)
    {
        Debug.Assert(CurrentNode is not null);

        var outputIndex = valueOutput.Index;
        writeValue(CurrentNode.Id, outputIndex, value);
    }

    internal void Write<T>(ValueOutputList<T> valueOutputList, int listIndex, T value)
    {
        Debug.Assert(CurrentNode is not null);

        var outputIndex = valueOutputList.Index;
        writeValueList(CurrentNode.Id, outputIndex, listIndex, value);
    }

    internal T ReadStore<T>(ContextStore<T> contextStore)
    {
        Debug.Assert(CurrentNode is not null);

        if (!stores.TryGetValue(CurrentNode.Id, out var refStore))
        {
            return default!;
        }

        return (T)refStore[contextStore].GetValue()!;
    }

    internal void WriteStore<T>(ContextStore<T> contextStore, T value)
    {
        Debug.Assert(CurrentNode is not null);

        stores.TryAdd(CurrentNode.Id, new Dictionary<IStore, IRef>());
        stores[CurrentNode.Id][contextStore] = new Ref<T>(value);
    }

    internal void CreateMemory(Node node)
    {
        var metadata = node.Metadata;

        var valueOutputRefs = new IRef[metadata.OutputsCount];

        for (var i = 0; i < metadata.OutputsCount; i++)
        {
            var outputMetadata = metadata.Outputs[i];

            if (metadata.ValueOutputHasVariableSize && i == metadata.OutputsCount - 1)
            {
                var arrSize = node.VariableSize.ValueOutputSize;
                var elementType = outputMetadata.Type;
                var arr = Array.CreateInstance(elementType, arrSize);
                var defaultValue = elementType.CreateDefault();

                for (var j = 0; j < arrSize; j++)
                {
                    arr.SetValue(defaultValue, i);
                }

                valueOutputRefs[i] = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(outputMetadata.Type.MakeArrayType()), args: [arr])!;
            }
            else
            {
                var defaultValue = outputMetadata.Type.CreateDefault();
                valueOutputRefs[i] = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(outputMetadata.Type), args: [defaultValue])!;
            }
        }

        memory.Peek()[node.Id] = valueOutputRefs;

        if (!hasMemory.Contains(node.Id))
            hasMemory.Add(node.Id);
    }
}

public interface IRef
{
    public object? GetValue();
    public Type GetValueType();
}

public class Ref<T> : IRef
{
    public T Value;

    public Ref(T startValue)
    {
        Value = startValue;
    }

    public object? GetValue() => Value;
    public Type GetValueType() => typeof(T);
}