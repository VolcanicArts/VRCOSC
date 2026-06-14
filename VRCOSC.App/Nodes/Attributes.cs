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

// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

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
public class NodeGenerics(params Type[] types) : Attribute
{
    public Type[] Types { get; } = types;
}

[AttributeUsage(AttributeTargets.Property)]
public class NodePropertyAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeCollapsedAttribute(params EFontAwesomeIcon[]? icons) : Attribute
{
    public EFontAwesomeIcon[]? Icons { get; } = icons;
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

[AttributeUsage(AttributeTargets.Field)]
public class InputMode(InputModes modes) : Attribute
{
    public InputModes Modes { get; } = modes;
}

public interface INodeElement
{
    Node Owner { get; set; }
    string Name { get; }
    INodeElementMetadata Metadata { get; internal set; }
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

public interface IValueInputBase : IValueElement;

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

public interface IValueOutputBase : IValueElement
{
    bool IsDirty { get; set; }
}

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

public abstract class NodeElement : INodeElement
{
    public Node Owner { get; set; } = null!;
    public INodeElementMetadata Metadata { get; set; } = null!;
    public string Name { get; }

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

public class FlowElement(string name = "") : NodeElement(name), IFlowElement;

public class FlowInput(string name = "") : FlowElement(name), IFlowInput
{
    public bool IsSource(IPulseContext c) => c.IsSource(this);
}

public class FlowInputList(string name = "") : FlowElement(name), IFlowInputList
{
    public int Count => Metadata.Size;

    public bool IsSource(int index, IPulseContext c) => c.IsSource(this, index);
}

public class FlowOutput(string name = "", bool scope = false) : FlowElement(name), IFlowOutput
{
    public bool Scope { get; } = scope;

    public Task Execute(IPulseContext c) => c.Execute(this);
}

public class FlowOutputList(string name = "", bool scope = false) : FlowElement(name), IFlowOutputList
{
    public bool Scope { get; } = scope;

    public int Count => Metadata.Size;

    public Task Execute(int index, IPulseContext c) => c.Execute(this, index);
}

public abstract class ValueElement<T>(string name = "") : NodeElement(name), IValueElement;

[Flags]
public enum InputModes
{
    Connection = 1 << 0,
    Inline = 1 << 1
}

public class ValueInput<T>([CallerMemberName] string name = "", T defaultValue = default!) : ValueElement<T>(name), IValueInput<T>
{
    public T DefaultValue { get; } = defaultValue;

    private T _field = defaultValue;

    public T Field
    {
        get => _field;
        set
        {
            _field = value;

            if (!Owner.Metadata.Shared.IsFlowInput && !Owner.Metadata.Shared.IsSelfUpdating)
                _ = Owner.ContainingGraph.TriggerTree(Owner);
        }
    }

    public T Read(IPulseContext c) => c.Read(this);

    public object? GetField() => _field;

    public void SetField(object? value) => _field = (T)value!;
}

public class ValueInputList<T>([CallerMemberName] string name = "") : ValueElement<T>(name), IValueInputList<T>
{
    public int Count => Metadata.Size;

    public IReadOnlyList<T> Read(IPulseContext c) => c.Read(this);
}

public class ValueOutput<T>([CallerMemberName] string name = "") : ValueElement<T>(name), IValueOutput<T>
{
    public bool IsDirty { get; set; }

    public void Write(T value, IPulseContext c) => c.Write(this, value);
}

public class ValueOutputList<T>([CallerMemberName] string name = "") : ValueElement<T>(name), IValueOutputList<T>
{
    public bool IsDirty { get; set; }
    public int Count => Metadata.Size;

    public void Write(int index, T value, IPulseContext c) => c.Write(this, index, value);
}

public class GlobalStore<T> : IGlobalStore<T>
{
    public Type Type => typeof(T);

    public void Write(T value, IPulseContext c) => c.Write(this, value);

    public T Read(IPulseContext c) => c.Read(this);
}

public interface IImpulseNode;

public interface IImpulseSender : IImpulseNode;

public interface IImpulseReceiver : IImpulseNode
{
    public bool CanReceive(string name, IPulseContext c);
    public void WriteOutputs(object[] values, IPulseContext c);
}

public interface IHasVariableReference
{
    public Guid VariableId { get; set; }
}

public record ImpulseDefinition(string Name, object[] Values);

internal interface IDisplayNode
{
    Action<object?>? OnValueChanged { get; set; }
    object? GetValue();
    void Clear();
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
/// If <see cref="OnUpdate"/> returns true it will process and notify nodes down flow of the <see cref="IValueOutputBase"/> changes, otherwise it will not process
/// </summary>
public interface IActiveUpdateNode
{
    int UpdateOffset { get; }
    Task<bool> OnUpdate(IPulseContext c);
}

/// <summary>
/// Processes this node every update, and if any <see cref="IValueOutputBase"/> has changed, notifies nodes down flow of the changes
/// </summary>
public interface IContinuousNode
{
    int UpdateOffset { get; }
}