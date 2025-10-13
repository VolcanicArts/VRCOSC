// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FontAwesome6;
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
public class NodePropertyAttribute : Attribute
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

    public Task Execute(PulseContext context) => context.Execute(this);
}

public class FlowContinuation : IFlow
{
    public int Index { get; set; }
    public string Name { get; init; }

    public FlowContinuation(string name = "")
    {
        Name = name;
    }

    public Task Execute(PulseContext context) => context.Execute(this);
}

public interface IStore;

public class ContextStore<T> : IStore
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
        return c.Graph.ReadStore(this, c);
    }

    public void Write(T value, PulseContext c)
    {
        c.Graph.WriteStore(this, value, c);
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
        return c.Peek().VariableSize.ValueOutputSize;
    }

    public void Write(int index, T value, PulseContext c)
    {
        c.Write(this, index, value);
    }
}

public interface IHasTextProperty
{
    public string Text { get; set; }
}

public interface IFlowInput;

public interface IImpulseNode;

public interface IImpulseSender : IImpulseNode;

public interface IImpulseReceiver : IImpulseNode, IHasTextProperty
{
    public void WriteOutputs(object[] values, PulseContext c);
}

public interface IHasVariableReference
{
    public Guid VariableId { get; set; }
}

public record ImpulseDefinition(string Name, object[] Values);

internal interface INodeEventHandler
{
    public bool HandleNodeStart(PulseContext c) => false;
    public bool HandleNodeStop(PulseContext c) => false;
    public bool HandleParameterReceive(PulseContext c, VRChatParameter parameter) => false;
    public bool HandleAvatarChange(PulseContext c, AvatarConfig? config) => false;
    public bool HandlePartialSpeechResult(PulseContext c, string result) => false;
    public bool HandleFinalSpeechResult(PulseContext c, string result) => false;
    public bool HandleOnInstanceJoined(PulseContext c, VRChatClientEventInstanceJoined eventArgs) => false;
    public bool HandleOnInstanceLeft(PulseContext c, VRChatClientEventInstanceLeft eventArgs) => false;
    public bool HandleOnUserJoined(PulseContext c, VRChatClientEventUserJoined eventArgs) => false;
    public bool HandleOnUserLeft(PulseContext c, VRChatClientEventUserLeft eventArgs) => false;
    public bool HandleOnAvatarPreChange(PulseContext c, VRChatClientEventAvatarPreChange eventArgs) => false;
}

internal interface IDisplayNode
{
    public void Clear();
}