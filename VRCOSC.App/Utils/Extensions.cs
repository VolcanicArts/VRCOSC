// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.App.Utils;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
            action(item);
    }
}

public static class EnumExtensions
{
    public static string ToLookup(this Enum @enum) => @enum.ToString().ToLowerInvariant();
}

public static class TypeExtensions
{
    public static string ToReadableName(this Type type)
    {
        if (type.IsSubclassOf(typeof(Enum))) return "Enum";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Empty => "Null",
            TypeCode.Object => "Object",
            TypeCode.DBNull => "DBNull",
            TypeCode.Boolean => "Bool",
            TypeCode.Char => "Char",
            TypeCode.SByte => "Byte",
            TypeCode.Byte => "UByte",
            TypeCode.Int16 => "Short",
            TypeCode.UInt16 => "UShort",
            TypeCode.Int32 => "Int",
            TypeCode.UInt32 => "UInt",
            TypeCode.Int64 => "Long",
            TypeCode.UInt64 => "ULong",
            TypeCode.Single => "Float",
            TypeCode.Double => "Double",
            TypeCode.Decimal => "Decimal",
            TypeCode.DateTime => "DateTime",
            TypeCode.String => "String",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown type provided")
        };
    }
}
