// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using VRCOSC.App.SDK.Nodes;
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

    internal List<Dictionary<Guid, IRef[]>> Memory { get; } = [new()];
    internal List<Dictionary<Guid, Dictionary<IStore, IRef>>> LocalStores { get; } = [];
    internal List<List<Guid>> RanNodes { get; } = [];
    internal List<Dictionary<ParameterDefinition, VRChatParameter>> ParameterCache { get; } = [];

    public int ScopePointer;

    internal PulseContext(NodeGraph graph)
    {
        Graph = graph;

        Source = new CancellationTokenSource();
        Token = Source.Token;

        Memory.Add(new Dictionary<Guid, IRef[]>());
        LocalStores.Add(new Dictionary<Guid, Dictionary<IStore, IRef>>());
        RanNodes.Add(new List<Guid>());
        ParameterCache.Add(new Dictionary<ParameterDefinition, VRChatParameter>());
    }

    internal bool HasRan(Guid nodeId)
    {
        // TODO: Allow value output only nodes (user input) to bypass the scope checking?
        // return RanNodes[ScopePointer].Contains(nodeId);

        for (var i = ScopePointer; i >= 0; i--)
        {
            if (RanNodes[i].Contains(nodeId)) return true;
        }

        return false;
    }

    internal void MarkRan(Guid nodeId)
    {
        RanNodes[ScopePointer].Add(nodeId);
    }

    internal void Execute(FlowCall call)
    {
        Memory.Add(new Dictionary<Guid, IRef[]>());
        LocalStores.Add(new Dictionary<Guid, Dictionary<IStore, IRef>>());
        RanNodes.Add(new List<Guid>());
        ParameterCache.Add(new Dictionary<ParameterDefinition, VRChatParameter>());
        ScopePointer++;

        processNext(call);

        Memory.Remove(Memory.Last());
        LocalStores.Remove(LocalStores.Last());
        RanNodes.Remove(RanNodes.Last());
        ParameterCache.Remove(ParameterCache.Last());
        ScopePointer--;
    }

    internal void Execute(FlowContinuation continuation)
    {
        processNext(continuation);
    }

    internal AvatarConfig? FindCurrentAvatar()
    {
        // TODO: Cache
        var avatarId = AppManager.GetInstance().VRChatOscClient.FindCurrentAvatar(Token).Result;
        // TODO: This shouldn't be here
        return avatarId is null ? null : AvatarConfigLoader.LoadConfigFor(avatarId);
    }

    internal T FindParameter<T>(string name) where T : unmanaged
    {
        var parameterDefinition = new ParameterDefinition(name, ParameterTypeFactory.CreateFrom<T>());

        // we use a local cache here to make sure that a parameter's value doesn't change while a flow is occurring as a single source could be connected to multiple nodes
        for (var i = ScopePointer; i >= 0; i--)
        {
            if (ParameterCache[i].TryGetValue(parameterDefinition, out var scopedParameter)) return scopedParameter.GetValue<T>();
        }

        var parameter = AppManager.GetInstance().FindParameter(parameterDefinition, Token).Result;
        if (parameter is not null) ParameterCache[ScopePointer][parameterDefinition] = parameter;

        return parameter?.GetValue<T>() ?? default!;
    }

    private void processNext(IFlow next)
    {
        Debug.Assert(CurrentNode is not null);

        var outputIndex = next.Index;
        var connection = Graph.FindConnectionFromFlowOutput(CurrentNode.Id, outputIndex);
        if (connection is null) return;

        Graph.ProcessNode(connection.InputNodeId, this);
    }

    private T readValue<T>(Guid nodeId, int index)
    {
        var metadata = Graph.Nodes[nodeId].Metadata;
        var hasVariableSize = metadata.ValueOutputHasVariableSize;

        for (var i = ScopePointer; i >= 0; i--)
        {
            var innerMemory = Memory[i];
            if (!innerMemory.TryGetValue(nodeId, out var refs)) continue;

            if (hasVariableSize && index >= metadata.OutputsCount - 1)
            {
                var variableRef = (T[])refs[metadata.OutputsCount - 1].GetValue()!;
                var subIndex = index - (metadata.OutputsCount - 1);
                return variableRef[subIndex];
            }

            return (T)refs[index].GetValue()!;
        }

        throw new InvalidOperationException();
    }

    private void writeValue<T>(Guid nodeId, int index, T value)
    {
        ((Ref<T>)Memory[ScopePointer][nodeId][index]).Value = value;
    }

    private void writeValueList<T>(Guid nodeId, int index, int listIndex, T value)
    {
        ((Ref<T[]>)Memory[ScopePointer][nodeId][index]).Value[listIndex] = value;
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

    internal T ReadStore<T>(LocalStore<T> localStore)
    {
        Debug.Assert(CurrentNode is not null);

        if (!LocalStores[ScopePointer].TryGetValue(CurrentNode.Id, out var refStore))
        {
            return default!;
        }

        return (T)refStore[localStore].GetValue()!;
    }

    internal void WriteStore<T>(LocalStore<T> localStore, T value)
    {
        Debug.Assert(CurrentNode is not null);

        LocalStores[ScopePointer].TryAdd(CurrentNode.Id, new Dictionary<IStore, IRef>());
        LocalStores[ScopePointer][CurrentNode.Id][localStore] = new Ref<T>(value);
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

        Memory[ScopePointer][node.Id] = valueOutputRefs;
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