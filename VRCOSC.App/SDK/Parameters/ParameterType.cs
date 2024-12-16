// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Parameters;

public enum ParameterType
{
    Bool,
    Int,
    Float
}

public static class ParameterTypeUtils
{
    public static ParameterType GetTypeFromType(Type type)
    {
        if (type == typeof(bool)) return ParameterType.Bool;
        if (type == typeof(int)) return ParameterType.Int;
        if (type == typeof(float)) return ParameterType.Float;

        throw new InvalidOperationException("Parameters can only be of type bool, int, or float");
    }

    public static ParameterType GetTypeFromType<T>() => GetTypeFromType(typeof(T));

    public static ParameterType GetTypeFromValue(object value) => GetTypeFromType(value.GetType());
}