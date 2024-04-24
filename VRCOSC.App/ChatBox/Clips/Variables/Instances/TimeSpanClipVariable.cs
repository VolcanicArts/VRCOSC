// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

public class TimeSpanClipVariable : ClipVariable
{
    public TimeSpanClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("time_format", "Time Format", "How should the time be formatted?")]
    public string TimeFormat { get; set; } = @"mm\:ss";

    [ClipVariableOption("include_negative_sign", "Include Negative Sign", "When a time span is negative, should we include a '-' sign?")]
    public bool IncludeNegativeSign { get; set; } = true;

    public override bool IsDefault() => base.IsDefault() && TimeFormat == @"mm\:ss";

    protected override string Format(object value)
    {
        var timeSpan = (TimeSpan)value;

        try
        {
            if (IncludeNegativeSign && timeSpan < TimeSpan.Zero)
                return $"-{timeSpan.ToString(TimeFormat)}";
            else
                return timeSpan.ToString(TimeFormat);
        }
        catch (Exception)
        {
            return "INCORRECT FORMAT";
        }
    }
}
