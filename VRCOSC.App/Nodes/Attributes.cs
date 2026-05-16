// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FontAwesome6;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute : Attribute
{
    public string Title { get; }
    public string Path { get; }

    public NodeAttribute(string title, string path = "")
    {
        if (string.IsNullOrWhiteSpace(title)) throw new Exception("A title must be provided for a node");

        Title = title;
        Path = path;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeGenerics : Attribute
{
    public Type[] Types { get; }

    public NodeGenerics(params Type[] types)
    {
        Types = types;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class NodePropertyAttribute : Attribute
{
    public string Name { get; }

    public NodePropertyAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeCollapsedAttribute : Attribute
{
    public EFontAwesomeIcon[]? Icons { get; }

    public NodeCollapsedAttribute(params EFontAwesomeIcon[]? icons)
    {
        Icons = icons;
    }
}

/// <inheritdoc />
/// <summary>
/// Forces a node to reprocess when its outputs requested. Good for source nodes to act as a ref instead of value
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NodeForceReprocessAttribute : Attribute;

/// <inheritdoc />
/// <summary>
/// Trigger nodes by default cancel an existing flow when processing. This stops that behaviour. Good for events coming from an API
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NodeNoCancelAttribute : Attribute;

public interface INodeElement
{
    Node Owner { get; set; }
    string Name { get; }
    INodeElementMetadata Metadata { get; }
    bool IsConnected { get; internal set; }
    Action? OnIsConnectedChanged { get; set; }
}

public interface IFlowElement : INodeElement;

public interface IFlowInputBase : IFlowElement;

public interface IFlowInput : IFlowInputBase;

public interface IFlowInputList : IFlowInputBase;

public interface IFlowOutputBase : IFlowElement
{
    bool Scope { get; }
}

public interface IFlowOutput : IFlowOutputBase;

public interface IFlowOutputList : IFlowOutputBase;

public interface IValueElement : INodeElement;

public interface IValueInputBase : IValueElement
{
    ValueInputMode Modes { get; }
}

public interface IValueInput : IValueInputBase
{
    object? GetField();
    void SetField(object? value);
}

public interface IValueInput<out T> : IValueInput
{
    T DefaultValue { get; }
    T Field { get; }
}

public interface IValueInputList : IValueInputBase
{
    int Count { get; }
}

public interface IValueInputList<out T> : IValueInputList;

public interface IValueOutputBase : IValueElement;

public interface IValueOutput : IValueOutputBase;

public interface IValueOutput<out T> : IValueOutput;

public interface IValueOutputList : IValueOutputBase;

public interface IValueOutputList<out T> : IValueOutputList;

public interface IStore
{
    Type Type { get; }
}

public interface IStore<T> : IStore;

public interface IGlobalStore<T> : IStore<T>;

public interface IContextStore : IStore;

public interface IContextStore<T> : IContextStore, IStore<T>;

public abstract record NodeElement : INodeElement
{
    public Node Owner { get; set; } = null!;
    public string Name { get; }
    public INodeElementMetadata Metadata => NodeMetadataManager.GetFor(Owner).Value.ElementMetadataFor(this);

    public bool IsConnected
    {
        get;
        set
        {
            if (EqualityComparer<bool>.Default.Equals(field, value)) return;

            field = value;
            OnIsConnectedChanged?.Invoke();
        }
    }

    public Action? OnIsConnectedChanged { get; set; }

    protected NodeElement(string name = "")
    {
        Name = name.ToSentence();
    }
}

public record FlowElement : NodeElement, IFlowElement
{
    protected FlowElement(string name = "")
        : base(name)
    {
    }
}

public record FlowInput : FlowElement, IFlowInput
{
    public FlowInput(string name = "")
        : base(name)
    {
    }

    public bool IsSource(IPulseContext c) => c.IsSource(this);
}

public record FlowInputList : FlowElement, IFlowInputList
{
    public FlowInputList([CallerMemberName] string name = "")
        : base(name)
    {
    }

    public int Count => Metadata.Size;

    public bool IsSource(int index, IPulseContext c) => c.IsSource(this, index);
}

public record FlowOutput : FlowElement, IFlowOutput
{
    public bool Scope { get; }

    public FlowOutput([CallerMemberName] string name = "", bool scope = false)
        : base(name)
    {
        Scope = scope;
    }

    public Task Execute(IPulseContext c) => c.Execute(this);
}

public record FlowOutputList : FlowElement, IFlowOutputList
{
    public bool Scope { get; }

    public FlowOutputList([CallerMemberName] string name = "", bool scope = false)
        : base(name)
    {
        Scope = scope;
    }

    public int Count => Metadata.Size;

    public Task Execute(int index, IPulseContext c) => c.Execute(this, index);
}

public abstract record ValueElement<T> : NodeElement, IValueElement
{
    protected ValueElement(string name = "")
        : base(name)
    {
    }
}

[Flags]
public enum ValueInputMode
{
    Connection = 1 << 0,
    Inline = 1 << 1
}

public record ValueInput<T> : ValueElement<T>, IValueInput<T>
{
    public T DefaultValue { get; }

    private T _field;

    public T Field
    {
        get => _field;
        set
        {
            _field = value;

            if (!Owner.Metadata.Shared.IsFlowInput)
                _ = Owner.ContainingGraph.TriggerTree(Owner);
        }
    }

    public ValueInputMode Modes { get; }

    public ValueInput([CallerMemberName] string name = "", T defaultValue = default!, ValueInputMode modes = ValueInputMode.Connection | ValueInputMode.Inline)
        : base(name)
    {
        DefaultValue = defaultValue;
        _field = defaultValue;
        Modes = modes;
    }

    public T Read(IPulseContext c) => !IsConnected ? _field : c.Read(this);

    public object? GetField() => _field;

    public void SetField(object? value) => _field = (T)value!;
}

public record ValueInputList<T> : ValueElement<T>, IValueInputList<T>
{
    public ValueInputMode Modes => ValueInputMode.Connection | ValueInputMode.Inline;

    public ValueInputList([CallerMemberName] string name = "")
        : base(name)
    {
    }

    public int Count => Metadata.Size;

    public IReadOnlyList<T> Read(IPulseContext c) => c.Read(this);
}

public record ValueOutput<T> : ValueElement<T>, IValueOutput<T>
{
    public ValueOutput([CallerMemberName] string name = "")
        : base(name)
    {
    }

    public void Write(T value, IPulseContext c) => c.Write(this, value);
}

public record ValueOutputList<T> : ValueElement<T>, IValueOutputList<T>
{
    public ValueOutputList([CallerMemberName] string name = "")
        : base(name)
    {
    }

    public int Count => Metadata.Size;

    public void Write(int index, T value, IPulseContext c) => c.Write(this, index, value);
}

public record GlobalStore<T> : IGlobalStore<T>
{
    public Type Type => typeof(T);

    public void Write(T value, IPulseContext c) => c.Write(this, value);

    public T Read(IPulseContext c) => c.Read(this);
}

public interface IImpulseNode;

public interface IImpulseSender : IImpulseNode;

public interface IImpulseReceiver : IImpulseNode
{
    public void WriteOutputs(object[] values, IPulseContext c);
}

public interface IHasVariableReference
{
    public Guid VariableId { get; set; }
}

public record ImpulseDefinition(string Name, object[] Values);

internal interface IDisplayNode
{
    public void Clear();
}

public interface IModuleNodeEventHandler
{
    public Task Write(object[] args, IPulseContext c);
}

/// <summary>
/// A passively updating node that can only read and write from stores in <see cref="OnUpdate"/>
/// </summary>
public interface IUpdateNode
{
    int UpdateOffset { get; }
    void OnUpdate(IPulseContext c);
}

/// <summary>
/// An actively updating node that can read inputs/stores and write outputs/stores in <see cref="OnUpdate"/>.
/// If <see cref="OnUpdate"/> returns true it will process and notify nodes down flow of the <see cref="ValueOutput{T}"/> changes, otherwise it will not process
/// </summary>
public interface IActiveUpdateNode
{
    int UpdateOffset { get; }
    Task<bool> OnUpdate(IPulseContext c);
}

/// <summary>
/// Processes this node every update, and if any value output has changed, notifies nodes down flow of the changes
/// </summary>
public interface IContinuousNode
{
    int UpdateOffset { get; }
}