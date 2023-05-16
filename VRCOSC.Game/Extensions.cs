// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Graphics;

namespace VRCOSC.Game;

public static class EnumExtensions
{
    public static string ToLookup(this Enum key) => key.ToString().ToLowerInvariant();
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

public static class TimeSpanExtensions
{
    public static string Format(this TimeSpan timeSpan) => string.Format(timeSpan.TotalHours >= 1 ? @"{0:hh\:mm\:ss}" : @"{0:mm\:ss}", timeSpan);
}

public static class ArrayExtensions
{
    public static T[] NewCopy<T>(this T[] source, int length)
    {
        var destination = new T[length];
        Array.Copy(source, destination, length);
        return destination;
    }
}

public static class Colour4Extensions
{
    public static Colour4 Invert(this Colour4 colour) => new(
        1f - colour.R,
        1f - colour.G,
        1f - colour.B,
        colour.A
    );
}
