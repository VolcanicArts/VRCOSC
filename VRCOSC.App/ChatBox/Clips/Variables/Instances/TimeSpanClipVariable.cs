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

    [ClipVariableOption("Time Format", "time_format")]
    public string TimeFormat { get; set; } = string.Empty;

    protected override string Format(object value)
    {
        if (string.IsNullOrEmpty(TimeFormat))
        {
            return $"{(TimeSpan)value:mm\\:ss}";
        }
        else
        {
            return ((TimeSpan)value).ToString(TimeFormat);
        }
    }
}
