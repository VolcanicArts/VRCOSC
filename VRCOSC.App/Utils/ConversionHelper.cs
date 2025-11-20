// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VRCOSC.App.Utils;

public static class ConversionHelper
{
    // Built‑in implicit numeric conversions (per the C# spec)
    private static readonly Dictionary<Type, Type[]> numeric_implicit = new()
    {
        {
            typeof(sbyte), new[]
            {
                typeof(short), typeof(int), typeof(long),
                typeof(float), typeof(double), typeof(decimal)
            }
        },
        {
            typeof(byte), new[]
            {
                typeof(short), typeof(ushort), typeof(int),
                typeof(uint), typeof(long), typeof(ulong),
                typeof(float), typeof(double), typeof(decimal)
            }
        },
        {
            typeof(short), new[]
            {
                typeof(int), typeof(long),
                typeof(float), typeof(double), typeof(decimal)
            }
        },
        {
            typeof(ushort), new[]
            {
                typeof(int), typeof(uint), typeof(long),
                typeof(ulong), typeof(float), typeof(double),
                typeof(decimal)
            }
        },
        {
            typeof(int), new[]
            {
                typeof(long), typeof(float),
                typeof(double), typeof(decimal)
            }
        },
        {
            typeof(uint), new[]
            {
                typeof(long), typeof(ulong),
                typeof(float), typeof(double),
                typeof(decimal)
            }
        },
        { typeof(long), new[] { typeof(float), typeof(double), typeof(decimal) } },
        { typeof(ulong), new[] { typeof(float), typeof(double), typeof(decimal) } },
        {
            typeof(char), new[]
            {
                typeof(ushort), typeof(int), typeof(uint),
                typeof(long), typeof(ulong), typeof(float),
                typeof(double), typeof(decimal)
            }
        },
        { typeof(float), new[] { typeof(double) } }
    };

    private const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public static bool HasImplicitConversion(Type source, Type target)
    {
        // Reference, boxing, identity, etc.
        if (target.IsAssignableFrom(source))
            return true;

        // Nullable<T> rule: T -> Nullable<T>
        if (target.IsGenericType
            && target.GetGenericTypeDefinition() == typeof(Nullable<>)
            && source == Nullable.GetUnderlyingType(target))
        {
            return true;
        }

        // User-defined implicit operators on either type
        if (hasUserDefinedImplicit(source, target) || hasUserDefinedImplicit(target, target, sourceIsParamForTarget: true))
            return true;

        // Built-in numeric conversions
        if (numeric_implicit.TryGetValue(source, out var validTargets)
            && validTargets.Contains(target))
        {
            return true;
        }

        return false;
    }

    private static bool hasUserDefinedImplicit(Type type, Type returnType, bool sourceIsParamForTarget = false)
    {
        // If sourceIsParamForTarget==false, we look on 'type' for:
        //   public static implicit operator returnType(type x)
        // If true, we look on 'type' for:
        //   public static implicit operator returnType(source param)
        // (i.e. swapping which type defines the operator)
        var methods = type.GetMethods(flags)
                          .Where(m => m.Name == "op_Implicit" && m.ReturnType == returnType);

        foreach (var m in methods)
        {
            var p = m.GetParameters();

            if (p.Length == 1)
            {
                var pt = p[0].ParameterType;

                if (!sourceIsParamForTarget ? pt == type : pt == returnType)
                    return true;
            }
        }

        return false;
    }
}