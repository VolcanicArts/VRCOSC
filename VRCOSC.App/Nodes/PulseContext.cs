// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

internal record FlowSource(int Slot, int Index);

public interface IPulseContext
{
    bool IsCancelled { get; }
    double DeltaTime { get; }

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
    internal void WriteContextStore<T>(string name, T value);
    internal T ReadContextStore<T>(string name);
}

public class PulseContext : IPulseContext
{
    internal readonly CancellationTokenSource Source;
    internal readonly Dictionary<Guid, IRef[][]> Memory = [];
    private readonly NodeGraph _graph;
    private readonly PulseContext? _baseContext;
    private readonly Dictionary<string, IRef> _keyedStores = [];
    private readonly Stack<Guid> _exectution = [];
    private readonly Stack<FlowSource> _flowSources = [];

    internal PulseContext(NodeGraph graph)
    {
        _graph = graph;
        Source = new CancellationTokenSource();
    }

    internal PulseContext(PulseContext baseContext, NodeGraph graph, CancellationTokenSource? source = null)
    {
        _graph = graph;
        _baseContext = baseContext;
        Source = source ?? baseContext.Source;
    }

    internal void Push(Guid element) => _exectution.Push(element);
    internal void Pop() => _exectution.Pop();
    internal Guid Peek() => _exectution.Peek();

    public bool IsCancelled => Source.IsCancellationRequested;
    public double DeltaTime => DeltaTimeInternal != 0d ? DeltaTimeInternal : _baseContext?.DeltaTime ?? 0d;

    internal double DeltaTimeInternal { get; set; }

    public Task Run(Task task) => task.WaitAsync(Source.Token);
    public Task<T> Run<T>(Task<T> task) => task.WaitAsync(Source.Token);

    public void WriteContextStore<T>(string name, T value) => _keyedStores[name] = new Ref<T>(value);
    public T ReadContextStore<T>(string name) => tryReadKeyedStore<T>(name, out var value) ? value : default!;

    public async Task Execute(IFlowOutput flowOutput)
    {
        var currentId = Peek();
        var slot = flowOutput.Metadata.Shared.Slot;
        var scope = flowOutput.Scope;

        var connectionResult = _graph.FindConnectionFromFlowOutput(currentId, slot, 0);
        if (!connectionResult.IsSuccess) return;

        var connection = connectionResult.Value;
        var flowSource = new FlowSource(connection.InputSlot, connection.InputSlotIndex);

        _flowSources.Push(flowSource);
        await _graph.ProcessNode(connection.InputId, scope ? new PulseContext(this, _graph, Source) : this);
        _flowSources.Pop();
    }

    public Task Execute(IFlowOutputList flowOutputList, int index)
    {
        var currentId = Peek();
        var slot = flowOutputList.Metadata.Shared.Slot;
        var scope = flowOutputList.Scope;

        var connectionResult = _graph.FindConnectionFromFlowOutput(currentId, slot, index);
        if (!connectionResult.IsSuccess) return Task.CompletedTask;

        var connection = connectionResult.Value;
        return _graph.ProcessNode(connection.InputId, scope ? new PulseContext(this, _graph, Source) : this);
    }

    internal bool HasMemory(Guid nodeId) => Memory.ContainsKey(nodeId) || (_baseContext?.HasMemory(nodeId) ?? false);

    private bool tryReadKeyedStore<T>(string name, out T value)
    {
        if (_keyedStores.TryGetValue(name, out var localIRef))
        {
            value = (T)localIRef.GetValue()!;
            return true;
        }

        if (_baseContext?.tryReadKeyedStore<T>(name, out var baseValue) ?? false)
        {
            value = baseValue;
            return true;
        }

        value = default!;
        return true;
    }

    private bool tryGetMemoryEntry(Guid nodeId, [NotNullWhen(true)] out IRef[][]? memoryEntry)
    {
        if (Memory.TryGetValue(nodeId, out var localEntry))
        {
            memoryEntry = localEntry;
            return true;
        }

        if (_baseContext?.tryGetMemoryEntry(nodeId, out var baseEntry) ?? false)
        {
            memoryEntry = baseEntry;
            return true;
        }

        memoryEntry = null;
        return false;
    }

    private bool tryReadValue<T>(Guid nodeId, int slot, int index, out T value)
    {
        if (tryGetMemoryEntry(nodeId, out var memoryEntry))
        {
            value = (T)memoryEntry[slot][index].GetValue()!;
            return true;
        }

        if (_baseContext?.tryReadValue<T>(nodeId, slot, index, out var baseValue) ?? false)
        {
            value = baseValue;
            return true;
        }

        value = default!;
        return false;
    }

    private void writeValue<T>(Guid nodeId, int slot, int index, T value)
    {
        var memoryResult = tryGetMemoryEntry(nodeId, out var memoryEntry);
        Debug.Assert(memoryResult && memoryEntry is not null);

        var iRefStore = memoryEntry[slot][index];
        Debug.Assert(iRefStore.ValueType == typeof(T));

        var refStore = (Ref<T>)iRefStore;
        refStore.Value = value;
    }

    public T Read<T>(IValueInput<T> valueInput)
    {
        var metadata = valueInput.Metadata;

        if (metadata.Shared.Modes == InputModes.Inline) return valueInput.Field;

        var currentId = Peek();
        var slot = valueInput.Metadata.Shared.Slot;

        var connectionResult = _graph.FindConnectionFromValueInput(currentId, slot, 0);
        if (!connectionResult.IsSuccess) return valueInput.Field;

        var connection = connectionResult.Value;
        var result = tryReadValue<T>(connection.OutputId, connection.OutputSlot, connection.OutputSlotIndex, out var value);
        return result ? value : valueInput.Field;
    }

    public IReadOnlyList<T> Read<T>(IValueInputList<T> valueInputList)
    {
        var currentId = Peek();
        var slot = valueInputList.Metadata.Shared.Slot;
        var size = valueInputList.Metadata.Size;
        var values = new T[size];

        for (var slotIndex = 0; slotIndex < size; slotIndex++)
        {
            var connectionResult = _graph.FindConnectionFromValueInput(currentId, slot, slotIndex);

            if (!connectionResult.IsSuccess)
            {
                values[slotIndex] = default!;
            }
            else
            {
                var connection = connectionResult.Value;
                var result = tryReadValue<T>(connection.OutputId, connection.OutputSlot, connection.OutputSlotIndex, out var value);
                values[slotIndex] = result ? value : default!;
            }
        }

        return values;
    }

    public void Write<T>(IValueOutput<T> valueOutput, T value) => writeValue(Peek(), valueOutput.Metadata.Shared.Slot, 0, value);

    public void Write<T>(IValueOutputList<T> valueOutputList, int slotIndex, T value) => writeValue(Peek(), valueOutputList.Metadata.Shared.Slot, slotIndex, value);

    public void Write<T>(IGlobalStore<T> store, T value) => _graph.WriteStore(store, value, this);

    public T Read<T>(IGlobalStore<T> store) => _graph.ReadStore(store, this);

    public bool IsSource(IFlowInput flowInput) => _flowSources.Peek().Slot == flowInput.Metadata.Shared.Slot;

    public bool IsSource(IFlowInputList flowInputList, int index)
    {
        var flowSource = _flowSources.Peek();
        return flowSource.Slot == flowInputList.Metadata.Shared.Slot && flowSource.Index == index;
    }

    internal void CreateMemory(INode node)
    {
        if (HasMemory(node.Id)) return;

        var valueOutputCount = node.Metadata.Shared.ValueOutputCount;
        Memory.Add(node.Id, new IRef[valueOutputCount][]);

        var valueOutputs = node.Metadata.ElementInstancesFor(ConnectionPoint.ValueOutput);

        for (var slot = 0; slot < valueOutputCount; slot++)
        {
            var valueOutput = valueOutputs[slot];
            var metadata = valueOutput.Metadata;
            var memoryEntry = new IRef[metadata.WorkingSize];

            for (var slotIndex = 0; slotIndex < memoryEntry.Length; slotIndex++)
            {
                memoryEntry[slotIndex] = (IRef)Activator.CreateInstance(typeof(Ref<>).MakeGenericType(metadata.Shared.ValueType))!;
            }

            Memory[node.Id][slot] = memoryEntry;
        }
    }
}