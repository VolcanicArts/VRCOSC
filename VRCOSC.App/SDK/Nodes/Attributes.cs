// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeAttribute : Attribute
{
    public string Title { get; }
    public string Path { get; }

    public NodeAttribute(string title, string path)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new Exception("A title must be provided for a node");
        //if (string.IsNullOrWhiteSpace(path)) throw new Exception("A path must be provided for a node");

        Title = title;
        Path = path;
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
public class NodeProcessAttribute : Attribute
{
    public string[] Inputs { get; }
    public string[] Outputs { get; }

    public NodeProcessAttribute(string[] inputs, string[] outputs)
    {
        Inputs = inputs;
        Outputs = outputs;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class NodeTriggerAttribute : Attribute
{
}