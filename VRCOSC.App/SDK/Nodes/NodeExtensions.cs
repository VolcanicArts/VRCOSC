// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Reflection;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public static class NodeExtensions
{
    public static MethodInfo GetProcessMethod(this Node node)
    {
        return node.GetType()
                   .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .Single(methodInfo => methodInfo.HasCustomAttribute<NodeProcessAttribute>());
    }
}