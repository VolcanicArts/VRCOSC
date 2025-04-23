// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;

namespace VRCOSC.App.SDK.Nodes.Types.Converters;

[Node("To String", "Converters")]
public sealed class ToStringNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("*")] T o,
        [NodeValue("Format Provider")] IFormatProvider? formatProvider,
        [NodeValue("Format")] string? format,
        [NodeValue("String")] ref string outString
    )
    {
        formatProvider ??= CultureInfo.CurrentCulture;

        if (o is null)
        {
            outString = "null";
            return;
        }

        if (o is IFormattable formattable)
        {
            outString = formattable.ToString(format, formatProvider);
            return;
        }

        outString = o.ToString() ?? "UNKNOWN";
    }
}