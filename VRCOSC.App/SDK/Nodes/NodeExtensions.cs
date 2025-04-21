// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public static class NodeExtensions
{
    public static bool IsFlowNode(this Node node, ConnectionSide side)
    {
        return (side.HasFlag(ConnectionSide.Input) && node.GetType().HasCustomAttribute<NodeFlowInputAttribute>())
               || (side.HasFlag(ConnectionSide.Output) && node.GetType().HasCustomAttribute<NodeFlowOutputAttribute>());
    }

    public static bool IsValueNode(this Node node, ConnectionSide side)
    {
        return (side.HasFlag(ConnectionSide.Input) && node.GetType().HasCustomAttribute<NodeValueInputAttribute>())
               || (side.HasFlag(ConnectionSide.Output) && node.GetType().HasCustomAttribute<NodeValueOutputAttribute>());
    }

    public static bool IsFlowNode<T>(ConnectionSide side) where T : Node
    {
        return (side.HasFlag(ConnectionSide.Input) && typeof(T).HasCustomAttribute<NodeFlowInputAttribute>())
               || (side.HasFlag(ConnectionSide.Output) && typeof(T).HasCustomAttribute<NodeFlowOutputAttribute>());
    }

    public static bool IsValueNode<T>(ConnectionSide side) where T : Node
    {
        return (side.HasFlag(ConnectionSide.Input) && typeof(T).HasCustomAttribute<NodeValueInputAttribute>())
               || (side.HasFlag(ConnectionSide.Output) && typeof(T).HasCustomAttribute<NodeValueOutputAttribute>());
    }

    public static NodeFlowInputAttribute GetFlowInputAttribute(this Node node)
    {
        var attribute = node.GetType().GetCustomAttribute<NodeFlowInputAttribute>();
        Debug.Assert(attribute is not null);
        return attribute;
    }

    public static NodeFlowOutputAttribute GetFlowOutputAttribute(this Node node)
    {
        var attribute = node.GetType().GetCustomAttribute<NodeFlowOutputAttribute>();
        Debug.Assert(attribute is not null);
        return attribute;
    }

    public static NodeValueInputAttribute GetValueInputAttribute(this Node node)
    {
        var attribute = node.GetType().GetCustomAttribute<NodeValueInputAttribute>();
        Debug.Assert(attribute is not null);
        return attribute;
    }

    public static NodeValueOutputAttribute GetValueOutputAttribute(this Node node)
    {
        var attribute = node.GetType().GetCustomAttribute<NodeValueOutputAttribute>();
        Debug.Assert(attribute is not null);
        return attribute;
    }

    public static List<MethodInfo> GetProcessMethods(this Node node)
    {
        return node.GetType()
                   .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .Where(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>())
                   .ToList();
    }
}