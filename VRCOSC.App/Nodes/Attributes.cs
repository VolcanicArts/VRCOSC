// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FontAwesome6;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute : Attribute
{
    public string Title { get; }
    public string? Path { get; }
    public EFontAwesomeIcon Icon { get; }

    internal NodeAttribute(string title, string path, EFontAwesomeIcon icon = EFontAwesomeIcon.None)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new Exception("A title must be provided for a node");

        Title = title;
        Path = path;
        Icon = icon;
    }

    public NodeAttribute(string title, EFontAwesomeIcon icon = EFontAwesomeIcon.None)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new Exception("A title must be provided for a node");

        Title = title;
        Icon = icon;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeGenericTypeFilterAttribute : Attribute
{
    public Type[] Types { get; }

    public NodeGenericTypeFilterAttribute(Type[] types)
    {
        Types = types;
    }
}

[AttributeUsage(AttributeTargets.Property)]
internal class NodePropertyAttribute : Attribute
{
    public string SerialisedName { get; }

    public NodePropertyAttribute(string serialisedName)
    {
        SerialisedName = serialisedName;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeCollapsedAttribute : Attribute;

/// <inheritdoc />
/// <summary>
/// Causes a node to trigger whenever the value changes. Should only be used on flow output only nodes
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class NodeReactiveAttribute : Attribute;

/// <inheritdoc />
/// <summary>
/// Forces a node to reprocess when its outputs requested. Good for source nodes to act as a ref instead of value
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NodeForceReprocessAttribute : Attribute;

public interface INodeAttribute
{
    public int Index { get; internal set; }
    public string Name { get; init; }
}

public interface IFlow : INodeAttribute;

public class FlowCall : IFlow
{
    public int Index { get; set; }
    public string Name { get; init; }

    public FlowCall(string name = "")
    {
        Name = name;
    }

    public void Execute(PulseContext context) => context.Execute(this);
}

public class FlowContinuation : IFlow
{
    public int Index { get; set; }
    public string Name { get; init; }

    public FlowContinuation(string name = "")
    {
        Name = name;
    }

    public void Execute(PulseContext context) => context.Execute(this);
}

public interface IStore;

public class LocalStore<T> : IStore
{
    public T Read(PulseContext c)
    {
        return c.ReadStore(this);
    }

    public void Write(T value, PulseContext c)
    {
        c.WriteStore(this, value);
    }
}

public class GlobalStore<T> : IStore
{
    public T Read(PulseContext c)
    {
        return c.Field.ReadStore(this, c);
    }

    public void Write(T value, PulseContext c)
    {
        c.Field.WriteStore(this, value, c);
    }
}

public interface IValueInput : INodeAttribute;

public interface IValueOutput : INodeAttribute;

public class ValueInput<T> : IValueInput
{
    public int Index { get; set; }
    public string Name { get; init; }

    internal T DefaultValue { get; }

    public ValueInput(string name = "", T defaultValue = default!)
    {
        Name = name;
        DefaultValue = defaultValue;
    }

    public T Read(PulseContext c)
    {
        return c.Read(this);
    }
}

public class ValueOutput<T> : IValueOutput
{
    public int Index { get; set; }
    public string Name { get; init; }

    public ValueOutput(string name = "")
    {
        Name = name;
    }

    public void Write(T value, PulseContext c)
    {
        c.Write(this, value);
    }
}

public class ValueInputList<T> : IValueInput
{
    public int Index { get; set; }
    public string Name { get; init; }

    public ValueInputList(string name = "")
    {
        Name = name;
    }

    public List<T> Read(PulseContext c)
    {
        return c.Read(this);
    }
}

public class ValueOutputList<T> : IValueOutput
{
    public int Index { get; set; }
    public string Name { get; init; }

    public ValueOutputList(string name = "")
    {
        Name = name;
    }

    public int Length(PulseContext c)
    {
        // TODO: Decouple this from the context as these won't change during running
        Debug.Assert(c.CurrentNode is not null);

        return c.CurrentNode.VariableSize.ValueOutputSize;
    }

    public void Write(int index, T value, PulseContext c)
    {
        c.Write(this, index, value);
    }
}

public interface IFlowInput;

public interface IImpulseNode
{
    public string Name { get; set; }
}

public interface IImpulseSender : IImpulseNode;

public interface IImpulseReceiver : IImpulseNode
{
    public void WriteOutputs(object[] values, PulseContext c);
}

public record ImpulseDefinition(string Name, object[] Values);

internal interface INodeEventHandler
{
    public bool HandleNodeStart(PulseContext c) => false;
    public bool HandleNodeStop(PulseContext c) => false;
    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter) => false;
    public bool HandleAvatarChange(PulseContext c, AvatarConfig? config) => false;
}

internal interface IUpdateNode
{
    public bool HasChanged(PulseContext c);
}

/// <summary>
/// Processes this node at 60hz, and then processes and updates downstream trigger nodes of the update if the result of <see cref="GetValue"/> has changed
/// </summary>
/// <remarks>This is useful for output values that need to be polled</remarks>
public abstract class UpdateNode<T> : Node, IUpdateNode
{
    private readonly GlobalStore<T> prevValue = new();

    public bool HasChanged(PulseContext c)
    {
        var value = GetValue(c);

        if (EqualityComparer<T>.Default.Equals(value, prevValue.Read(c))) return false;

        prevValue.Write(value, c);
        return true;
    }

    protected abstract T GetValue(PulseContext c);
}