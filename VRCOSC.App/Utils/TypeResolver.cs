// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

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
        var baseType = resolveTypeFromName(className);
        if (baseType is null) return null;

        var constructedType = baseType;

        if (baseType.IsGenericType)
        {
            if (string.IsNullOrEmpty(generics)) return null;
            if (!TryConstructGenericType(generics, baseType, out var constructedGenericType)) return null;

            constructedType = constructedGenericType;
        }

        return constructedType;
    }

    private static (string TypeName, string Generics) extractTypeNameAndGenerics(string friendlyName)
    {
        if (friendlyName == null)
            throw new ArgumentNullException(nameof(friendlyName));

        var m = FriendlyNameRegex().Match(friendlyName.Trim());

        if (!m.Success)
            throw new FormatException($"Invalid type declaration: '{friendlyName}'");

        var baseName = m.Groups["name"].Value;

        var generics = m.Groups["gen"].Success
            ? m.Groups["gen"].Value
            : string.Empty;

        // If there are generic args, count them and append `N
        if (!string.IsNullOrEmpty(generics))
        {
            int count = splitTopLevel(generics, ',', '<', '>').Count;
            baseName = $"{baseName}`{count}";
        }

        return (baseName, generics);
    }

    // Helper: splits `s` on `sep` only when depth between `open`/`close` is zero.
    private static List<string> splitTopLevel(string s, char sep, char open, char close)
    {
        var parts = new List<string>();
        int depth = 0, last = 0;

        for (int i = 0; i < s.Length; i++)
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

        var rawNames = genericCsl.Split(',');

        var typeArgs = new List<Type>(rawNames.Length);

        foreach (var raw in rawNames)
        {
            var name = raw.Trim();
            var wantsNullable = name.EndsWith('?');

            if (wantsNullable)
            {
                name = name[..^1];
            }

            var baseType = resolveTypeFromName(name);

            if (baseType == null || baseType.IsAbstract)
            {
                constructedType = null;
                return false;
            }

            Type finalType = baseType;

            if (wantsNullable && baseType.IsValueType)
            {
                finalType = typeof(Nullable<>).MakeGenericType(baseType);
            }

            typeArgs.Add(finalType);
        }

        try
        {
            constructedType = openGenericType.MakeGenericType(typeArgs.ToArray());
            return true;
        }
        catch
        {
            return false;
        }
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

    [GeneratedRegex("^(?<name>[^<]+)(?:<(?<gen>.+)>)?$")]
    private static partial Regex FriendlyNameRegex();
}