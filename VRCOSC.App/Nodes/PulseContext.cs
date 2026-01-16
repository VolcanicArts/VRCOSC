// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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
    public CancellationToken Token => Source.Token;

    public bool IsCancelled => Token.IsCancellationRequested;

    internal readonly NodeGraph Graph;
    internal readonly PulseContext? BaseContext;
    private Dictionary<Guid, IRef[]> memory { get; } = [];
    private Dictionary<Guid, Dictionary<IStore, IRef>> stores { get; } = [];
    private Dictionary<string, IRef> keyedStores { get; } = [];
    private Stack<Node> nodes { get; } = [];

    internal PulseContext(NodeGraph graph)
    {
        Graph = graph;
        Source = new CancellationTokenSource();
    }

    internal PulseContext(PulseContext baseContext, NodeGraph graph, CancellationTokenSource? source = null)
    {
        Graph = graph;
        BaseContext = baseContext;
        Source = source ?? baseContext.Source;
    }

    internal void Push(Node node) => nodes.Push(node);

    internal void Pop() => nodes.Pop();

    internal Node Peek() => nodes.Peek();

    internal bool HasMemory(Guid nodeId) => memory.ContainsKey(nodeId) || (BaseContext?.HasMemory(nodeId) ?? false);

    internal Task Execute(FlowCall call) => processNext(call, true);

    internal Task Execute(FlowContinuation continuation) => processNext(continuation, false);

    internal Task<AvatarConfig?> GetCurrentAvatar() => AppManager.GetInstance().FindCurrentAvatar(Token);

    internal VRChatClient GetClient() => AppManager.GetInstance().VRChatClient;

    internal Player GetPlayer() => AppManager.GetInstance().VRChatClient.Player;

    internal Instance GetInstance() => AppManager.GetInstance().VRChatClient.Instance;

    internal UserCamera GetUserCamera() => AppManager.GetInstance().VRChatClient.UserCamera;

    internal string GetSpeechText() => Graph.CurrentSpeechText;

    internal async Task<VRChatParameter?> GetParameter<T>(string name)
    {
        var parameterDefinition = new ParameterDefinition(name, ParameterTypeFactory.CreateFrom<T>());
        return await AppManager.GetInstance().FindParameter(parameterDefinition, Token);
    }

    private Task processNext(IFlow next, bool scope)
    {
        var current = Peek();

        var outputIndex = next.Index;
        var connection = Graph.FindConnectionFromFlowOutput(current.Id, outputIndex);
        return connection is null ? Task.CompletedTask : Graph.ProcessNode(connection.InputNodeId, scope ? new PulseContext(this, Graph) : this);
    }

    private T readValue<T>(Guid nodeId, int index)
    {
        var metadata = Graph.Nodes[nodeId].Metadata;
        var hasVariableSize = metadata.ValueOutputHasVariableSize;

        if (memory.TryGetValue(nodeId, out var refs))
        {
            if (hasVariableSize && index >= metadata.OutputsCount - 1)
            {
                var variableRef = (T[])refs[metadata.OutputsCount - 1].GetValue()!;
                var subIndex = index - (metadata.OutputsCount - 1);
                return variableRef[subIndex];
            }

            return (T)refs[index].GetValue()!;
        }

        if (BaseContext is not null)
            return BaseContext.readValue<T>(nodeId, index);

        // In theory this should never happen, but sometimes it does because of race conditions, so we'll just return default for at most 1 update
        return default!;
    }

    private void writeValue<T>(Guid nodeId, int index, T value)
    {
        ((Ref<T>)memory[nodeId][index]).Value = value;
    }

    private void writeValueList<T>(Guid nodeId, int index, int listIndex, T value)
    {
        ((Ref<T[]>)memory[nodeId][index]).Value[listIndex] = value;
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

    internal void WriteKeyedStore<T>(string key, T value)
    {
        keyedStores[key] = new Ref<T>(value);
    }

    internal T ReadKeyedStore<T>(string key)
    {
        if (keyedStores.TryGetValue(key, out var @ref))
            return @ref.GetValueType() == typeof(T) ? (T)@ref.GetValue()! : default!;

        if (BaseContext is not null)
            return BaseContext.ReadKeyedStore<T>(key);

        return default!;
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

        memory[node.Id] = valueOutputRefs;
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