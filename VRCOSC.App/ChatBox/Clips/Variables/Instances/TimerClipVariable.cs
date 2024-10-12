// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.ChatBox.Clips.Variables.Instances;

internal class TimerClipVariable : TimeSpanClipVariable
{
    public TimerClipVariable()
    {
    }

    public TimerClipVariable(ClipVariableReference reference)
        : base(reference)
    {
    }

    [ClipVariableOption("datetime", "Date/Time", "The chosen date/time to count to")]
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;

    protected override string Format(object value)
    {
        var timespan = DateTime - DateTimeOffset.Now;
        return base.Format(timespan);
    }
}
