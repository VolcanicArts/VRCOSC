// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes;

public record FlowSource(IFlowInputBase FlowInput, int Index);

public interface IPulseContext
{
    bool IsCancelled { get; }

    Task Run(Task task);
    Task<T> Run<T>(Task<T> task);

    internal Task Execute(IFlowOutput flowOutput);
    internal Task Execute(IFlowOutputList flowOutputList, int slotIndex);
    internal bool IsSource(IFlowInput flowInput);
    internal bool IsSource(IFlowInputList flowInputList, int slotIndex);
    internal T Read<T>(IValueInput<T> valueInput);
    internal IReadOnlyList<T> Read<T>(IValueInputList<T> valueInputList);
    internal void Write<T>(IValueOutput<T> valueOutput, T value);
    internal void Write<T>(IValueOutputList<T> valueOutputList, int slotIndex, T value);
    internal void Write<T>(IGlobalStore<T> store, T value);
    internal T Read<T>(IGlobalStore<T> store);
    internal void WriteKeyedStore<T>(string name, T value);
    internal T ReadKeyedStore<T>(string name);
}

public class PulseContext : IPulseContext
{
    internal CancellationTokenSource Source { get; }
    public CancellationToken Token => Source.Token;

    public bool IsCancelled => Token.IsCancellationRequested;

    internal readonly NodeGraph Graph;
    internal readonly PulseContext? BaseContext;
    internal Dictionary<Guid, IRef[][]> Memory { get; } = [];
    private Dictionary<Guid, Dictionary<IStore, IRef>> stores { get; } = [];
    private Dictionary<string, IRef> keyedStores { get; } = [];
    private Stack<INode> nodes { get; } = [];
    private Stack<FlowSource> flowSources { get; } = [];

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

    internal void Push(INode node) => nodes.Push(node);

    internal void Pop() => nodes.Pop();

    internal INode Peek() => nodes.Peek();

    internal bool HasMemory(Guid nodeId) => Memory.ContainsKey(nodeId) || (BaseContext?.HasMemory(nodeId) ?? false);

    public Task Run(Task task) => task.WaitAsync(Source.Token);
    public Task<T> Run<T>(Task<T> task) => task.WaitAsync(Source.Token);

    internal VRChatClient GetClient() => AppManager.GetInstance().VRChatClient;
    internal string? GetSpeechText() => Graph.CurrentSpeechText;
    internal VRChatParameter? GetParameter<T>(string name) => AppManager.GetInstance().GetParameter<T>(name);
    internal TemplatedVRChatParameter? GetParameter<T>(Regex pattern) => AppManager.GetInstance().GetParameter<T>(pattern);

    public void WriteKeyedStore<T>(string name, T value)
    {
    }

    public T ReadKeyedStore<T>(string name)
    {
        return default!;
    }

    public async Task Execute(IFlowOutput flowOutput)
    {
        var current = Peek();
        var slot = flowOutput.Metadata.Shared.Slot;
        var scope = flowOutput.Scope;

        var connectionResult = Graph.FindConnectionFromFlowOutput(current.Id, slot, 0);
        if (!connectionResult.IsSuccess) return;

        var connection = connectionResult.Value;

        var inputNodeResult = Graph.GetNode(connection.InputId);
        Debug.Assert(inputNodeResult.IsSuccess);
        var inputNode = inputNodeResult.Value;
        var flowInputElement = (IFlowInputBase)inputNode.Metadata.ElementInstancesFor(ConnectionPoint.FlowInput)[connection.InputSlot];
        var flowSource = new FlowSource(flowInputElement, connection.InputSlot);

        flowSources.Push(flowSource);
        await Graph.ProcessNode(connection.InputId, scope ? new PulseContext(this, Graph) : this);
        flowSources.Pop();
    }

    public Task Execute(IFlowOutputList flowOutputList, int index)
    {
        var current = Peek();
        var slot = flowOutputList.Metadata.Shared.Slot;
        var scope = flowOutputList.Scope;

        var connectionResult = Graph.FindConnectionFromFlowOutput(current.Id, slot, index);
        if (!connectionResult.IsSuccess) return Task.CompletedTask;

        var connection = connectionResult.Value;
        return Graph.ProcessNode(connection.InputId, scope ? new PulseContext(this, Graph) : this);
    }

    private bool tryReadValue<T>(Guid nodeId, int slot, int index, out T value)
    {
        if (Memory.TryGetValue(nodeId, out var memoryEntry))
        {
            Debug.Assert(memoryEntry is not null);

            value = (T)memoryEntry[slot][index].GetValue()!;
            return true;
        }

        if (BaseContext is not null)
        {
            var baseResult = BaseContext.tryReadValue<T>(nodeId, slot, index, out var baseValue);

            if (baseResult)
            {
                value = baseValue;
                return true;
            }
        }

        value = default!;
        return false;
    }

    private void writeValue<T>(Guid nodeId, int slot, int index, T value)
    {
        var memoryResult = Memory.TryGetValue(nodeId, out var memoryEntry);
        Debug.Assert(memoryResult && memoryEntry is not null);

        var iRefStore = memoryEntry[slot][index];
        Debug.Assert(iRefStore.ValueType == typeof(T));

        var refStore = (Ref<T>)iRefStore;
        refStore.Value = value;
    }

    public T Read<T>(IValueInput<T> valueInput)
    {
        if (valueInput.Modes == ValueInputMode.Inline) return valueInput.Field;

        var current = Peek();
        var slot = valueInput.Metadata.Shared.Slot;

        var connectionResult = Graph.FindConnectionFromValueInput(current.Id, slot, 0);
        if (!connectionResult.IsSuccess) return valueInput.Field;

        var connection = connectionResult.Value;
        var result = tryReadValue<T>(connection.OutputId, connection.OutputSlot, connection.OutputSlotIndex, out var value);
        return result ? value : valueInput.Field;
    }

    public IReadOnlyList<T> Read<T>(IValueInputList<T> valueInputList)
    {
        var current = Peek();
        var slot = valueInputList.Metadata.Shared.Slot;
        var size = valueInputList.Metadata.Size;
        var values = new T[size];

        for (var i = 0; i < size; i++)
        {
            var connectionResult = Graph.FindConnectionFromValueInput(current.Id, slot, i);

            if (!connectionResult.IsSuccess)
            {
                values[i] = default!;
            }
            else
            {
                var connection = connectionResult.Value;
                var result = tryReadValue<T>(connection.OutputId, connection.OutputSlot, connection.OutputSlotIndex, out var value);
                values[i] = result ? value : default!;
            }
        }

        return values;
    }

    public void Write<T>(IValueOutput<T> valueOutput, T value)
    {
        var current = Peek();
        var slot = valueOutput.Metadata.Shared.Slot;
        writeValue(current.Id, slot, 0, value);
    }

    public void Write<T>(IValueOutputList<T> valueOutputList, int slotIndex, T value)
    {
        var current = Peek();
        var slot = valueOutputList.Metadata.Shared.Slot;
        writeValue(current.Id, slot, slotIndex, value);
    }

    public void Write<T>(IGlobalStore<T> store, T value) => Graph.WriteStore(store, value, this);

    public T Read<T>(IGlobalStore<T> store) => Graph.ReadStore(store, this);

    internal void CreateMemory(INode node)
    {
        if (Memory.ContainsKey(node.Id)) return;

        var valueOutputCount = node.Metadata.Shared.ValueOutputCount;
        Memory.Add(node.Id, new IRef[valueOutputCount][]);

        var valueOutputs = node.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput);

        for (var i = 0; i < valueOutputCount; i++)
        {
            var valueOutput = valueOutputs[i];
            var metadata = valueOutput.Metadata;
            var memoryEntry = new IRef[metadata.WorkingSize];

            for (var j = 0; j < memoryEntry.Length; j++)
            {
                memoryEntry[j] = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(metadata.Shared.ValueType))!;
            }

            Memory[node.Id][i] = memoryEntry;
        }
    }

    public bool IsSource(IFlowInput flowInput)
    {
        var flowSource = flowSources.Peek();
        return flowSource.FlowInput == flowInput;
    }

    public bool IsSource(IFlowInputList flowInputList, int index)
    {
        var flowSource = flowSources.Peek();
        return flowSource.FlowInput == flowInputList && flowSource.Index == index;
    }
}

public interface IRef
{
    public Type ValueType { get; }
    public object? GetValue();
}

public class Ref<T> : IRef
{
    public T Value;
    public Type ValueType => typeof(T);

    public Ref(T startValue = default!)
    {
        Value = startValue;
    }

    // Required for Activator.CreateInstance
    public Ref()
    {
        Value = default!;
    }

    public object? GetValue() => Value;

    public override bool Equals(object? obj) => obj is Ref<T> otherRef && EqualityComparer<T>.Default.Equals(Value, otherRef.Value);
}