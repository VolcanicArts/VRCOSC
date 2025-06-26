// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.Utils;

public static partial class TypeResolver
{
    public static readonly Dictionary<string, Type> TYPE_ALIASES = new(StringComparer.OrdinalIgnoreCase)
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

    private static Lazy<Dictionary<string, Type>> typeIndex = new(buildTypeIndex, true);

    public static void Reset() => typeIndex = new(buildTypeIndex, true);

    public static Type? Construct(string friendlyName)
    {
        var (className, generics) = extractTypeNameAndGenerics(friendlyName);

        if (string.IsNullOrWhiteSpace(className))
            return null;

        var baseType = resolveTypeFromName(className);
        if (baseType is null) return null;

        var constructedType = baseType;

        if (baseType.IsGenericType)
        {
            if (string.IsNullOrWhiteSpace(generics)) return null;
            if (!TryConstructGenericType(generics, baseType, out var constructedGenericType)) return null;

            constructedType = constructedGenericType;
        }

        return constructedType;
    }

    private static (string TypeName, string Generics) extractTypeNameAndGenerics(string friendlyName)
    {
        if (string.IsNullOrWhiteSpace(friendlyName))
            throw new ArgumentNullException(nameof(friendlyName));

        string trimmed = friendlyName.Trim();

        int genStart = trimmed.IndexOf('<');

        // No generics present
        if (genStart < 0)
            return (trimmed, string.Empty);

        // Incomplete/malformed generic — return (null, null) to signal invalid format
        if (!trimmed.EndsWith('>'))
            return (string.Empty, string.Empty); // signal error to Construct()

        string baseName = trimmed[..genStart];
        string generics = trimmed[(genStart + 1)..^1]; // between < >

        // Count generic args
        var segments = splitTopLevel(generics, ',', '<', '>');
        baseName = $"{baseName}`{segments.Count}";

        return (baseName, generics);
    }

    // Helper: splits `s` on `sep` only when depth between `open`/`close` is zero.
    private static List<string> splitTopLevel(string s, char sep, char open, char close)
    {
        var parts = new List<string>();
        int depth = 0, last = 0;

        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == open) depth++;
            else if (s[i] == close) depth--;
            else if (s[i] == sep && depth == 0)
            {
                parts.Add(s.Substring(last, i - last).Trim());
                last = i + 1;
            }
        }

        // final segment
        parts.Add(s[last..].Trim());
        return parts;
    }

    public static bool TryConstructGenericType(string genericCsl, Type openGenericType, [NotNullWhen(true)] out Type? constructedType)
    {
        constructedType = null;

        if (string.IsNullOrWhiteSpace(genericCsl))
            return false;

        var rawNames = splitTopLevel(genericCsl, ',', '<', '>');

        var typeArgs = new List<Type>(rawNames.Count);

        foreach (var raw in rawNames)
        {
            var name = raw.Trim();

            if (string.IsNullOrEmpty(name))
            {
                constructedType = null;
                return false; // Invalid generic parameter (e.g. trailing comma)
            }

            var wantsNullable = name.EndsWith('?');

            if (wantsNullable)
                name = name[..^1];

            var resolved = Construct(name); // Recursively resolve nested generics

            if (resolved == null || resolved.IsAbstract)
            {
                constructedType = null;
                return false;
            }

            Type finalType = resolved;

            if (wantsNullable && resolved.IsValueType)
            {
                finalType = typeof(Nullable<>).MakeGenericType(resolved);
            }

            typeArgs.Add(finalType);
        }

        try
        {
            if (!SatisfiesConstraints(openGenericType, typeArgs))
            {
                constructedType = null;
                return false;
            }

            constructedType = openGenericType.MakeGenericType(typeArgs.ToArray());
            return true;
        }
        catch
        {
            constructedType = null;
            return false;
        }
    }

    private static bool SatisfiesConstraints(Type openGenericType, List<Type> typeArgs)
    {
        if (!openGenericType.IsGenericTypeDefinition)
            return true;

        var genericParams = openGenericType.GetGenericArguments();

        for (int i = 0; i < genericParams.Length; i++)
        {
            var param = genericParams[i];
            var arg = typeArgs[i];

            // Base type/interface constraints
            foreach (var constraint in param.GetGenericParameterConstraints())
            {
                var constructedConstraint = constraint;

                if (constructedConstraint.IsGenericType && constructedConstraint.ContainsGenericParameters)
                {
                    constructedConstraint = constructedConstraint.GetGenericTypeDefinition().MakeGenericType(typeArgs.ToArray());
                }

                if (!constructedConstraint.IsAssignableFrom(arg))
                    return false;
            }

            var attrs = param.GenericParameterAttributes;

            if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && arg.IsValueType)
                return false;

            if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
                (!arg.IsValueType || Nullable.GetUnderlyingType(arg) != null))
                return false;

            if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                if (arg.IsAbstract || arg.GetConstructor(Type.EmptyTypes) == null)
                    return false;
            }
        }

        return true;
    }

    private static Type? resolveTypeFromName(string name)
    {
        name = name.Trim();

        if (TYPE_ALIASES.TryGetValue(name, out var p))
            return p;

        var direct = Type.GetType(name, throwOnError: false, ignoreCase: true);

        if (direct != null)
            return direct;

        typeIndex.Value.TryGetValue(name, out var found);
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
                if (t.IsAbstract) continue;

                if (t.FullName != null) dict.TryAdd(t.FullName, t);

                dict.TryAdd(t.Name, t);
            }
        }

        return dict;
    }
}