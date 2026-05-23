// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using VRCOSC.App.SDK.Utils;

namespace VRCOSC.App.Nodes;

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
        typeof(float),
        typeof(double),
        typeof(decimal),
    };

    public static readonly Type[] TEXTBOX_TYPES = new[]
    {
        typeof(string)
    }.Concat(NUMERIC_TYPES).ToArray();

    public static readonly Type[] INPUT_TYPES = new[]
    {
        typeof(bool),
        typeof(Keybind),
        typeof(DateTime),
        typeof(TimeSpan)
    }.Concat(TEXTBOX_TYPES).ToArray();

    public static bool IsInputType(Type type) => INPUT_TYPES.Contains(type) || type.IsEnum;
}