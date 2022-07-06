// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.Util;

public static class TypeUtils
{
    public static string TypeToReadableName<T>()
    {
        var typeCode = Type.GetTypeCode(typeof(T));

        return typeCode switch
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
            _ => throw new ArgumentOutOfRangeException($"Unknown type provided of type {nameof(T)}")
        };
    }
}
