// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

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

    public static Type? ResolveType(string name)
    {
        name = name.Trim();

        if (PRIMITIVE_TYPE_ALIASES.TryGetValue(name, out var primitiveType))
            return primitiveType;

        var direct = Type.GetType(name, throwOnError: false, ignoreCase: true);

        if (direct != null)
            return direct;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in safeGetTypes(asm))
            {
                if (string.Equals(type.FullName, name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }
        }

        return null;
    }

    private static IEnumerable<Type> safeGetTypes(Assembly asm)
    {
        try
        {
            return asm.GetTypes();
        }
        catch
        {
            return Enumerable.Empty<Type>();
        }
    }

    public static bool TryConstructGenericType(string userInput, Type openGenericType, [NotNullWhen(true)] out Type? constructedType)
    {
        constructedType = null;

        var typeNames = userInput.Split(',')
                                 .Select(t => t.Trim())
                                 .ToArray();

        var typeArgs = typeNames
                       .Select(ResolveType)
                       .ToArray();

        if (typeArgs.Any(t => t == null))
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
}