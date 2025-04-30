// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using FontAwesome6;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute : Attribute
{
    public string? Title { get; }
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

[AttributeUsage(AttributeTargets.Method)]
public class NodeProcessAttribute : Attribute;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class NodeVariableSizeAttribute : Attribute
{
    public int DefaultSize { get; }

    public NodeVariableSizeAttribute(int defaultSize = 2)
    {
        DefaultSize = defaultSize;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class NodeValueAttribute : Attribute
{
    public string Name { get; }

    public NodeValueAttribute(string name = "")
    {
        Name = name;
    }
}

/// <summary>
/// Causes a node to trigger whenever the value changes. Should only be used on flow output only nodes
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class NodeReactiveAttribute : Attribute;

public record NodeFlowRef(string Name = "", bool Scope = false);

public interface IFlowInput;

public interface IFlowOutput
{
    public NodeFlowRef[] FlowOutputs { get; }
}

public interface IAnyParameterReceiver
{
    public void OnAnyParameterReceived(ReceivedParameter parameter);
}