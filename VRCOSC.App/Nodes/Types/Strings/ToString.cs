// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("To String", "Strings")]
public sealed class ToStringNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("*")] T o,
        [NodeValue("Format Provider")] IFormatProvider? formatProvider,
        [NodeValue("Format")] string? format,
        [NodeValue("String")] Ref<string> outString
    )
    {
        formatProvider ??= CultureInfo.CurrentCulture;

        if (o is null)
        {
            outString.Value = "null";
            return;
        }

        try
        {
            if (o is IFormattable formattable)
            {
                outString.Value = formattable.ToString(format, formatProvider);
                return;
            }
        }
        catch
        {
            return;
        }

        outString.Value = o.ToString() ?? "UNKNOWN";
    }
}

[Node("String Format", "Strings")]
public sealed class StringFormatNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Format")] string? format,
        [NodeValue("Values")] [NodeVariableSize] object?[] values,
        [NodeValue("String")] Ref<string?> outString
    )
    {
        if (format is null) return;

        try
        {
            outString.Value = string.Format(format, values);
        }
        catch
        {
        }
    }
}