// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute : Attribute
{
    public string Title { get; }

    public NodeAttribute(string title)
    {
        Title = title;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeFlowInputAttribute : Attribute
{
    public bool IsTrigger { get; }

    public NodeFlowInputAttribute(bool isTrigger = false)
    {
        IsTrigger = isTrigger;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeFlowOutputAttribute : Attribute
{
    public List<string> Titles { get; }

    public NodeFlowOutputAttribute(params string[] titles)
    {
        Titles = titles.ToList();
    }

    public int Count => Titles.Count;
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeFlowLoop : Attribute
{
    public int[] FlowSlots { get; }

    public NodeFlowLoop(params int[] flowSlots)
    {
        FlowSlots = flowSlots;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeValueInputAttribute : Attribute
{
    public List<string> Titles { get; }

    public NodeValueInputAttribute(params string[] titles)
    {
        Titles = titles.ToList();
    }

    public int Count => Titles.Count;
}

[AttributeUsage(AttributeTargets.Class)]
public class NodeValueOutputAttribute : Attribute
{
    public List<Type> OutputTypes { get; }

    public NodeValueOutputAttribute(params Type[] outputTypes)
    {
        OutputTypes = outputTypes.ToList();
    }

    public int Count => OutputTypes.Count;
}

[AttributeUsage(AttributeTargets.Method)]
public class NodeProcessAttribute : Attribute
{
    public NodeProcessAttribute()
    {
    }
}