// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Parameters;

public enum ParameterType
{
    Bool = 0,
    Int = 1,
    Float = 2
}

public static class ParameterTypeFactory
{
    public static ParameterType CreateFrom(Type type)
    {
        if (type == typeof(bool)) return ParameterType.Bool;
        if (type == typeof(int)) return ParameterType.Int;
        if (type == typeof(float)) return ParameterType.Float;

        throw new InvalidOperationException("Parameters can only be of type bool, int, or float");
    }

    /// <summary>
    /// Takes in <typeparamref name="T"/> and attempts to create a <see cref="ParameterType"/> from it
    /// </summary>
    public static ParameterType CreateFrom<T>() => CreateFrom(typeof(T));

    /// <summary>
    /// Takes in <paramref name="value"/> and attempts to create a <see cref="ParameterType"/> from its <see cref="Type"/>
    /// </summary>
    public static ParameterType CreateFrom(object value) => CreateFrom(value.GetType());
}