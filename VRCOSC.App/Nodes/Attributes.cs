// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FontAwesome6;
using VRCOSC.App.SDK.Parameters;

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

[AttributeUsage(AttributeTargets.Class)]
public class NodeCollapsedAttribute : Attribute;

/// <summary>
/// Causes a node to trigger whenever the value changes. Should only be used on flow output only nodes
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class NodeReactiveAttribute : Attribute;

public interface INodeField
{
    public int Index { get; internal set; }
}

public interface IFlow : INodeField
{
    public string Name { get; init; }
}

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

public interface IValueInput : INodeField;

public interface IValueOutput : INodeField;

public class ValueInput<T> : IValueInput
{
    public int Index { get; set; }

    internal T DefaultValue { get; }

    public ValueInput(T defaultValue = default!)
    {
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

internal interface IParameterHandler
{
    public bool HandlesParameter(ReceivedParameter parameter);
}