// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.Util;

public static class TypeUtils
{
    public static string TypeToReadableName<T>()
    {
        var typeCode = Type.GetTypeCode(typeof(T));

        switch (typeCode)
        {
            case TypeCode.Boolean:
                return "Bool";

            case TypeCode.Int32:
                return "Int";

            case TypeCode.Single:
                return "Float";

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
