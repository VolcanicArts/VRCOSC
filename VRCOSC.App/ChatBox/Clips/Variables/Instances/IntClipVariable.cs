// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class IntClipVariable : ClipVariable
{
    public IntClipVariable()
    {
    }

    public IntClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("mode", "Mode", "What mode should we use for formatting?")]
    public IntVariableMode Mode { get; set; } = IntVariableMode.Standard;

    [ClipVariableOption("min_value", "Min Value", "The minimum value for when the symbol list should start interpolating\nRequires Symbol mode")]
    public int MinValue { get; set; } = 0;

    [ClipVariableOption("max_value", "Max Value", "The maximum value for when the symbol list should finish interpolating\nRequires Symbol mode")]
    public int MaxValue { get; set; } = 100;

    [ClipVariableOption("symbol_list", "Symbols", "The symbols to interpolate between\nRequires Symbol mode")]
    public List<string> SymbolList { get; set; } = new();

    public override bool IsDefault() => base.IsDefault() && Mode == IntVariableMode.Standard && MinValue == 0 && MaxValue == 100 && SymbolList.Count == 0;

    protected override string Format(object value)
    {
        var intValue = (int)value;

        if (Mode == IntVariableMode.Standard)
        {
            return intValue == int.MaxValue ? "\u221e" : intValue.ToString(CultureInfo.CurrentCulture);
        }

        if (Mode == IntVariableMode.Symbol)
        {
            var convertedInt = Math.Min(Math.Max(intValue, MinValue), MaxValue);
            var progress = (float)Interpolation.Map(convertedInt, MinValue, MaxValue, 0, 1);
            return getSymbol(progress);
        }

        return string.Empty;
    }

    private string getSymbol(float progress)
    {
        if (progress is < 0 or > 1) return string.Empty;

        var index = (int)MathF.Round(progress * (SymbolList.Count - 1));
        return SymbolList[index];
    }

    public enum IntVariableMode
    {
        Standard,
        Symbol
    }
}