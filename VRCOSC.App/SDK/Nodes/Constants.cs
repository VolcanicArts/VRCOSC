// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;

namespace VRCOSC.App.SDK.Nodes;

public static class NodeConstants
{
    public static readonly Type[] NUMERIC_TYPES = new[]
    {
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(nint),
        typeof(nuint),
        typeof(float),
        typeof(double),
        typeof(decimal),
    };

    public static readonly Type[] INPUT_TYPES = new[]
    {
        typeof(string),
        typeof(bool)
    }.Concat(NUMERIC_TYPES).ToArray();

    public static readonly Type[] PARAMETER_TYPES = new[]
    {
        typeof(bool),
        typeof(int),
        typeof(float)
    };
}