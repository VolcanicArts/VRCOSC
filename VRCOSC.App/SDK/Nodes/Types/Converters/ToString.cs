// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;

namespace VRCOSC.App.SDK.Nodes.Types.Converters;

[Node("To String", "Converters")]
public sealed class ToStringNode<T> : Node
{
    [NodeProcess(["*", "Culture Info", "Format"], ["String"])]
    private string process(T? o, CultureInfo? cultureInfo, string? format)
    {
        cultureInfo ??= CultureInfo.CurrentCulture;

        if (o is null) return "null";
        if (o is IFormattable formattable) return formattable.ToString(format, cultureInfo);

        return o.ToString() ?? "UNKNOWN";
    }
}