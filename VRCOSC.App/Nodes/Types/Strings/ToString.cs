// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("To String", "Strings")]
public sealed class ToStringNode<T> : ValueComputeNode<string>
{
    public ValueInput<T> Value = new();
    public ValueInput<string> Format = new();
    public ValueInput<IFormatProvider> FormatProvider = new();

    protected override string ComputeValue(PulseContext c)
    {
        var value = Value.Read(c);
        var format = Format.Read(c);
        var formatProvider = FormatProvider.Read(c);

        if (value is null) return "null";
        if (value is IFormattable formattable) return formattable.ToString(format, formatProvider);

        return value.ToString() ?? "UNKNOWN";
    }
}

[Node("String Format", "Strings")]
public sealed class StringFormatNode : ValueComputeNode<string>
{
    public ValueInput<string?> Format = new();
    public ValueInputList<object?> Values = new();

    protected override string ComputeValue(PulseContext c)
    {
        try
        {
            return string.Format(Format.Read(c) ?? string.Empty, Values.Read(c).ToArray());
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}