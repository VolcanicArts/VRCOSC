// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("TimeSpan Extract", "DateTime")]
public sealed class TimeSpanExtractNode : Node
{
    public ValueInput<TimeSpan> TimeSpan = new();

    public ValueOutput<int> Days = new();
    public ValueOutput<int> Hours = new();
    public ValueOutput<int> Minutes = new();
    public ValueOutput<int> Seconds = new();
    public ValueOutput<int> Milliseconds = new();
    public ValueOutput<int> Microseconds = new();
    public ValueOutput<int> Nanoseconds = new();

    protected override Task Process(PulseContext c)
    {
        var timeSpan = TimeSpan.Read(c);

        Days.Write(timeSpan.Days, c);
        Hours.Write(timeSpan.Hours, c);
        Minutes.Write(timeSpan.Minutes, c);
        Seconds.Write(timeSpan.Seconds, c);
        Milliseconds.Write(timeSpan.Milliseconds, c);
        Microseconds.Write(timeSpan.Microseconds, c);
        Nanoseconds.Write(timeSpan.Nanoseconds, c);

        return Task.CompletedTask;
    }
}

[Node("TimeSpan Extract Total", "DateTime")]
public sealed class TimeSpanExtractTotalNode : Node
{
    public ValueInput<TimeSpan> TimeSpan = new();

    public ValueOutput<double> Days = new();
    public ValueOutput<double> Hours = new();
    public ValueOutput<double> Minutes = new();
    public ValueOutput<double> Seconds = new();
    public ValueOutput<double> Milliseconds = new();
    public ValueOutput<double> Microseconds = new();
    public ValueOutput<double> Nanoseconds = new();

    protected override Task Process(PulseContext c)
    {
        var timeSpan = TimeSpan.Read(c);

        Days.Write(timeSpan.TotalDays, c);
        Hours.Write(timeSpan.TotalHours, c);
        Minutes.Write(timeSpan.TotalMinutes, c);
        Seconds.Write(timeSpan.TotalSeconds, c);
        Milliseconds.Write(timeSpan.TotalMilliseconds, c);
        Microseconds.Write(timeSpan.TotalMicroseconds, c);
        Nanoseconds.Write(timeSpan.TotalNanoseconds, c);

        return Task.CompletedTask;
    }
}

[Node("DateTime Extract", "DateTime")]
public sealed class DateTimeExtractNode : Node
{
    public ValueInput<DateTime> DateTime = new();

    public ValueOutput<int> DayOfYear = new("Day Of Year");
    public ValueOutput<int> Year = new();
    public ValueOutput<int> Month = new();
    public ValueOutput<int> Day = new();
    public ValueOutput<int> Hour = new();
    public ValueOutput<int> Minute = new();
    public ValueOutput<int> Second = new();
    public ValueOutput<int> Millisecond = new();
    public ValueOutput<int> Microsecond = new();
    public ValueOutput<int> Nanosecond = new();

    protected override Task Process(PulseContext c)
    {
        var dateTime = DateTime.Read(c);

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

        return Task.CompletedTask;
    }
}