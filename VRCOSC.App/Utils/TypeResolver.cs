// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VRCOSC.App.Utils;

public static class TypeResolver
{
    public static readonly Dictionary<string, Type> PRIMITIVE_TYPE_ALIASES =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "char", typeof(char) },
            { "decimal", typeof(decimal) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) },
            { "object", typeof(object) },
            { "string", typeof(string) }
        };

    // Build this dictionary once, on first access:
    // Key: either FullName or Name (both case-insensitive), Value: the Type
    // TODO: Reinitialise this when modules are reloaded
    private static readonly Lazy<Dictionary<string, Type>> type_index = new(buildTypeIndex, true);

    public static bool TryConstructGenericType(string userInput, Type openGenericType, [NotNullWhen(true)] out Type? constructedType)
    {
        constructedType = null;

        var typeArgs = userInput.Split(',').Select(t => resolveType(t.Trim())).ToArray();

        if (typeArgs.Any(t => t == null || t.IsAbstract))
            return false;

        try
        {
            constructedType = openGenericType.MakeGenericType(typeArgs!);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Type? resolveType(string name)
    {
        name = name.Trim();

        // 1) primitives
        if (PRIMITIVE_TYPE_ALIASES.TryGetValue(name, out var p))
            return p;

        // 2) fully-qualified via the runtime
        var direct = Type.GetType(name, throwOnError: false, ignoreCase: true);

        if (direct != null)
            return direct;

        // 3) O(1) lookup in our cached index
        type_index.Value.TryGetValue(name, out var found);
        return found;
    }

    private static Dictionary<string, Type> buildTypeIndex()
    {
        var dict = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.IsDynamic) continue;

            Type[] types;

            try
            {
                types = asm.GetExportedTypes();
            }
            catch
            {
                continue;
            }

            foreach (var t in types)
            {
                if (t.IsAbstract || t.IsGenericTypeDefinition) continue;

                // 1) full name
                if (t.FullName != null) dict.TryAdd(t.FullName, t);

                // 2) simple name (e.g. "DateTime")
                dict.TryAdd(t.Name, t);
            }
        }

        return dict;
    }
}