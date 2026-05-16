// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using VRCOSC.App.Nodes.Metadata;
using VRCOSC.App.Nodes.Types;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public static partial class NodeTypeManager
{
    public static Dictionary<string, NodeTypeMetadata> Data { get; } = [];

    public static void Init()
    {
        var types = getAllNodeTypes();
        var groupedTypes = types.GroupBy(t => NamespaceRegex().Match(t.GetFriendlyName(true)).Value);

        foreach (var groupedType in groupedTypes)
        {
            var linkedTypes = groupedType.Select(t => t).ToArray();
            var metadataResult = NodeMetadataManager.GetFor(linkedTypes[0]);

            Data[groupedType.Key] = new NodeTypeMetadata
            {
                Shared = metadataResult.Value,
                LinkedTypes = linkedTypes
            };
        }
    }

    private static IEnumerable<Type> getAllNodeTypes()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = asm.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var t in types)
            {
                if (t.IsAbstract) continue;
                if (!typeof(Node).IsAssignableFrom(t)) continue;
                if (!t.IsAssignableTo(typeof(Node))) continue;

                yield return t;
            }
        }
    }

    [GeneratedRegex("^[^<]+")]
    private static partial Regex NamespaceRegex();
}

public record NodeTypeMetadata
{
    public required INodeSharedMetadata Shared { get; init; }
    public required Type[] LinkedTypes { get; init; }
}