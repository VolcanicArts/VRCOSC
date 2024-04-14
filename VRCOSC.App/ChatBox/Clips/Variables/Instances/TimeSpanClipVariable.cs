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

    public override bool IsDefault() => base.IsDefault() && TimeFormat == @"mm\:ss";

    protected override string Format(object value)
    {
        return ((TimeSpan)value).ToString(TimeFormat).Replace("-", string.Empty);
    }
}
