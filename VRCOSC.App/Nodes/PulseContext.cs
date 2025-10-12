// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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
    private Stack<Dictionary<Guid, IRef[]>> memory { get; init; } = [];
    private Dictionary<Guid, Dictionary<IStore, IRef>> stores { get; init; } = [];
    private Stack<Node> nodes { get; } = [];

    internal PulseContext(NodeGraph graph)
    {
        Graph = graph;

        Source = new CancellationTokenSource();
        Token = Source.Token;

        memory.Push(new Dictionary<Guid, IRef[]>());
    }

    #region DeepCopy

    internal PulseContext DeepCopy()
    {
        var c = new PulseContext(Graph)
        {
            memory = cloneMemory(memory),
            stores = cloneStores(stores)
        };

        foreach (var node in nodes)
        {
            c.Push(node);
        }

        return c;
    }

    private static Stack<Dictionary<Guid, IRef[]>> cloneMemory(Stack<Dictionary<Guid, IRef[]>> memory)
    {
        // Stack<T>.ToArray() returns top->bottom
        var layers = memory.ToArray();
        var clone = new Stack<Dictionary<Guid, IRef[]>>(layers.Length);

        for (int i = layers.Length - 1; i >= 0; i--) // push bottom->top to keep same order/top
            clone.Push(cloneGuidRefArrayMap(layers[i]));

        return clone;
    }

    private static Dictionary<Guid, Dictionary<IStore, IRef>> cloneStores(
        Dictionary<Guid, Dictionary<IStore, IRef>> stores)
    {
        var outer = new Dictionary<Guid, Dictionary<IStore, IRef>>(stores.Count, stores.Comparer);

        foreach (var (guid, inner) in stores)
            outer[guid] = cloneStoreMap(inner);

        return outer;
    }

    private static Dictionary<Guid, IRef[]> cloneGuidRefArrayMap(Dictionary<Guid, IRef[]> src)
    {
        var dst = new Dictionary<Guid, IRef[]>(src.Count, src.Comparer);

        foreach (var (id, refs) in src)
            dst[id] = cloneRefArray(refs);
        return dst;
    }

    private static Dictionary<IStore, IRef> cloneStoreMap(Dictionary<IStore, IRef> src)
    {
        var dst = new Dictionary<IStore, IRef>(src.Count, src.Comparer);

        foreach (var (store, aref) in src)
            dst[store] = cloneRef(aref);
        return dst;
    }

    private static IRef[] cloneRefArray(IRef[] src)
    {
        if (src is null) return Array.Empty<IRef>();

        var dst = new IRef[src.Length];

        for (int i = 0; i < src.Length; i++)
            dst[i] = cloneRef(src[i]);
        return dst;
    }

    private static IRef cloneRef(IRef source)
    {
        var valueType = source.GetValueType();
        var value = source.GetValue();

        var concrete = typeof(Ref<>).MakeGenericType(valueType);
        return (IRef)Activator.CreateInstance(concrete, value)!;
    }

    #endregion

    internal void Push(Node node) => nodes.Push(node);

    internal void Pop() => nodes.Pop();

    internal Node Peek() => nodes.Peek();

    internal bool HasMemory(Guid nodeId) => memory.Any(dict => dict.ContainsKey(nodeId));

    internal void PushMemory() => memory.Push(new Dictionary<Guid, IRef[]>());

    internal void PopMemory() => memory.Pop();

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
        var current = Peek();

        var outputIndex = next.Index;
        var connection = Graph.FindConnectionFromFlowOutput(current.Id, outputIndex);
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
        var current = Peek();

        var inputIndex = valueInput.Index;
        var connection = Graph.FindConnectionFromValueInput(current.Id, inputIndex);
        return connection is null ? valueInput.DefaultValue : readValue<T>(connection.OutputNodeId, connection.OutputSlot);
    }

    internal List<T> Read<T>(ValueInputList<T> valueInputList)
    {
        var current = Peek();

        var inputIndex = valueInputList.Index;

        var list = new List<T>();

        for (var i = inputIndex; i < current.VirtualValueInputCount(); i++)
        {
            var connection = Graph.FindConnectionFromValueInput(current.Id, i);
            list.Add(connection is null ? default! : readValue<T>(connection.OutputNodeId, connection.OutputSlot));
        }

        return list;
    }

    internal void Write<T>(ValueOutput<T> valueOutput, T value)
    {
        var current = Peek();

        var outputIndex = valueOutput.Index;
        writeValue(current.Id, outputIndex, value);
    }

    internal void Write<T>(ValueOutputList<T> valueOutputList, int listIndex, T value)
    {
        var current = Peek();

        var outputIndex = valueOutputList.Index;
        writeValueList(current.Id, outputIndex, listIndex, value);
    }

    internal T ReadStore<T>(ContextStore<T> contextStore)
    {
        var current = Peek();

        if (!stores.TryGetValue(current.Id, out var refStore))
        {
            return default!;
        }

        return (T)refStore[contextStore].GetValue()!;
    }

    internal void WriteStore<T>(ContextStore<T> contextStore, T value)
    {
        var current = Peek();

        stores.TryAdd(current.Id, new Dictionary<IStore, IRef>());
        stores[current.Id][contextStore] = new Ref<T>(value);
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