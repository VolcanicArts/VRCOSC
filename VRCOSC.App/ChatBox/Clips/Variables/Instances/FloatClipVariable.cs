// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class FloatClipVariable : ClipVariable
{
    public FloatClipVariable()
    {
    }

    public FloatClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("mode", "Mode", "What mode should we use for formatting?")]
    public FloatVariableMode Mode { get; set; } = FloatVariableMode.Standard;

    [ClipVariableOption("float_format", "Float Format", "How should the float be formatted?\nRequires Standard mode")]
    public string FloatFormat { get; set; } = "F1";

    [ClipVariableOption("symbol_list", "Symbols", "The symbols to interpolate between\nRequires Symbol mode")]
    public List<string> SymbolList { get; set; } = new();

    public override bool IsDefault() => base.IsDefault() && Mode == FloatVariableMode.Standard && FloatFormat == "F1" && SymbolList.Count == 0;

    public override FloatClipVariable Clone()
    {
        var clone = (FloatClipVariable)base.Clone();

        clone.FloatFormat = FloatFormat;

        return clone;
    }

    protected override string Format(object value)
    {
        var floatValue = (float)value;

        if (float.IsPositiveInfinity(floatValue)) return "\u221e";
        if (float.IsNegativeInfinity(floatValue)) return "-\u221e";

        if (Mode == FloatVariableMode.Standard)
        {
            try
            {
                return floatValue.ToString(FloatFormat, CultureInfo.CurrentCulture);
            }
            catch (Exception)
            {
                return "INVALID FORMAT";
            }
        }

        if (Mode == FloatVariableMode.Symbol)
        {
            return getSymbol(floatValue);
        }

        return string.Empty;
    }

    private string getSymbol(float progress)
    {
        if (progress is < 0 or > 1) return string.Empty;

        var index = (int)MathF.Round(progress * (SymbolList.Count - 1));
        return SymbolList[index];
    }

    public enum FloatVariableMode
    {
        Standard,
        Symbol
    }
}
