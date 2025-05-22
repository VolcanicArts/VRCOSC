// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("To String", "Strings")]
public sealed class ToStringNode<T> : Node
{
    public ValueInput<T> Value = new();
    public ValueInput<string> Format = new();
    public ValueInput<IFormatProvider> FormatProvider = new();
    public ValueOutput<string> Result = new();

    protected override void Process(PulseContext c)
    {
        var value = Value.Read(c);
        var format = Format.Read(c);
        var formatProvider = FormatProvider.Read(c);

        if (value is null)
        {
            Result.Write("null", c);
            return;
        }

        try
        {
            if (value is IFormattable formattable)
            {
                Result.Write(formattable.ToString(format, formatProvider), c);
                return;
            }
        }
        catch
        {
            return;
        }

        Result.Write(value.ToString() ?? "UNKNOWN", c);
    }
}

[Node("String Format", "Strings")]
public sealed class StringFormatNode : Node
{
    public ValueInput<string?> Format = new();
    public ValueInputList<object?> Values = new();
    public ValueOutput<string> Result = new();

    protected override void Process(PulseContext c)
    {
        try
        {
            Result.Write(string.Format(Format.Read(c) ?? string.Empty, Values.Read(c).ToArray()), c);
        }
        catch
        {
            Result.Write("INVALID FORMAT", c);
        }
    }
}