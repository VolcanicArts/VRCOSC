// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("TimeSpan Construct", "Date & Time")]
public sealed class TimeSpanConstructNode : ValueComputeNode<TimeSpan>
{
    public ValueInput<float> Days = new();
    public ValueInput<float> Hours = new();
    public ValueInput<float> Minutes = new();
    public ValueInput<float> Seconds = new();
    public ValueInput<float> Milliseconds = new();
    public ValueInput<float> Microseconds = new();

    protected override TimeSpan ComputeValue(PulseContext c)
    {
        try
        {
            var timeSpan = TimeSpan.Zero;
            timeSpan += TimeSpan.FromDays(Days.Read(c));
            timeSpan += TimeSpan.FromHours(Hours.Read(c));
            timeSpan += TimeSpan.FromMinutes(Minutes.Read(c));
            timeSpan += TimeSpan.FromSeconds(Seconds.Read(c));
            timeSpan += TimeSpan.FromMilliseconds(Milliseconds.Read(c));
            timeSpan += TimeSpan.FromMicroseconds(Microseconds.Read(c));
            return timeSpan;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }
}

[Node("TimeSpan Extract", "Date & Time")]
public sealed class TimeSpanExtractNode() : ValueConsumeNode<TimeSpan>("TimeSpan")
{
    public ValueOutput<int> Days = new();
    public ValueOutput<int> Hours = new();
    public ValueOutput<int> Minutes = new();
    public ValueOutput<int> Seconds = new();
    public ValueOutput<int> Milliseconds = new();
    public ValueOutput<int> Microseconds = new();
    public ValueOutput<int> Nanoseconds = new();

    protected override void ConsumeValue(TimeSpan timeSpan, PulseContext c)
    {
        Days.Write(timeSpan.Days, c);
        Hours.Write(timeSpan.Hours, c);
        Minutes.Write(timeSpan.Minutes, c);
        Seconds.Write(timeSpan.Seconds, c);
        Milliseconds.Write(timeSpan.Milliseconds, c);
        Microseconds.Write(timeSpan.Microseconds, c);
        Nanoseconds.Write(timeSpan.Nanoseconds, c);
    }
}

[Node("TimeSpan Extract Total", "Date & Time")]
public sealed class TimeSpanExtractTotalNode() : ValueConsumeNode<TimeSpan>("TimeSpan")
{
    public ValueOutput<double> Days = new();
    public ValueOutput<double> Hours = new();
    public ValueOutput<double> Minutes = new();
    public ValueOutput<double> Seconds = new();
    public ValueOutput<double> Milliseconds = new();
    public ValueOutput<double> Microseconds = new();
    public ValueOutput<double> Nanoseconds = new();

    protected override void ConsumeValue(TimeSpan timeSpan, PulseContext c)
    {
        Days.Write(timeSpan.TotalDays, c);
        Hours.Write(timeSpan.TotalHours, c);
        Minutes.Write(timeSpan.TotalMinutes, c);
        Seconds.Write(timeSpan.TotalSeconds, c);
        Milliseconds.Write(timeSpan.TotalMilliseconds, c);
        Microseconds.Write(timeSpan.TotalMicroseconds, c);
        Nanoseconds.Write(timeSpan.TotalNanoseconds, c);
    }
}

[Node("DateTime Construct", "Date & Time")]
public sealed class DateTimeConstructNode : ValueComputeNode<DateTime>
{
    public ValueInput<float> Days = new();
    public ValueInput<float> Hours = new();
    public ValueInput<float> Minutes = new();
    public ValueInput<float> Seconds = new();
    public ValueInput<float> Milliseconds = new();
    public ValueInput<float> Microseconds = new();

    protected override DateTime ComputeValue(PulseContext c)
    {
        try
        {
            var dateTime = DateTime.UnixEpoch;
            dateTime += TimeSpan.FromDays(Days.Read(c));
            dateTime += TimeSpan.FromHours(Hours.Read(c));
            dateTime += TimeSpan.FromMinutes(Minutes.Read(c));
            dateTime += TimeSpan.FromSeconds(Seconds.Read(c));
            dateTime += TimeSpan.FromMilliseconds(Milliseconds.Read(c));
            dateTime += TimeSpan.FromMicroseconds(Microseconds.Read(c));
            return dateTime;
        }
        catch
        {
            return DateTime.UnixEpoch;
        }
    }
}

[Node("DateTime Extract", "Date & Time")]
public sealed class DateTimeExtractNode() : ValueConsumeNode<DateTime>("DateTime")
{
    public ValueOutput<int> DayOfYear = new();
    public ValueOutput<int> Year = new();
    public ValueOutput<int> Month = new();
    public ValueOutput<int> Day = new();
    public ValueOutput<int> Hour = new();
    public ValueOutput<int> Minute = new();
    public ValueOutput<int> Second = new();
    public ValueOutput<int> Millisecond = new();
    public ValueOutput<int> Microsecond = new();
    public ValueOutput<int> Nanosecond = new();

    protected override void ConsumeValue(DateTime dateTime, PulseContext c)
    {
        DayOfYear.Write(dateTime.DayOfYear, c);
        Year.Write(dateTime.Year, c);
        Month.Write(dateTime.Month, c);
        Day.Write(dateTime.Day, c);
        Hour.Write(dateTime.Hour, c);
        Minute.Write(dateTime.Minute, c);
        Second.Write(dateTime.Second, c);
        Millisecond.Write(dateTime.Millisecond, c);
        Microsecond.Write(dateTime.Microsecond, c);
        Nanosecond.Write(dateTime.Nanosecond, c);
    }
}

[Node("DateTime Difference", "Date & Time")]
public sealed class DateTimeDifferenceNode() : SimpleResultComputeNode<DateTime, TimeSpan>((a, b) => a - b);

[Node("DateTime Add", "Date & Time", EFontAwesomeIcon.Solid_Plus)]
[NodeCollapsed]
public sealed class DateTimeAddNode() : SimpleResultComputeNode<DateTime, TimeSpan, DateTime>((a, b) => a.Add(b), "DateTime", "TimeSpan");

[Node("DateTime Subtract", "Date & Time", EFontAwesomeIcon.Solid_Minus)]
[NodeCollapsed]
public sealed class DateTimeSubtractNode() : SimpleResultComputeNode<DateTime, TimeSpan, DateTime>((a, b) => a.Subtract(b), "DateTime", "TimeSpan");

[Node("TimeSpan Add", "Date & Time", EFontAwesomeIcon.Solid_Plus)]
[NodeCollapsed]
public sealed class TimeSpanAddNode() : SimpleResultComputeNode<TimeSpan>((a, b) => a.Add(b), "Source", "Value");

[Node("TimeSpan Subtract", "Date & Time", EFontAwesomeIcon.Solid_Minus)]
[NodeCollapsed]
public sealed class TimeSpanSubtractNode() : SimpleResultComputeNode<TimeSpan>((a, b) => a.Subtract(b), "Source", "Value");