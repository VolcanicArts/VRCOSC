// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("TimeSpan Construct", "Date & Time")]
public sealed class TimeSpanConstructNode : Node
{
    public ValueInput<float> Days = new();
    public ValueInput<float> Hours = new();
    public ValueInput<float> Minutes = new();
    public ValueInput<float> Seconds = new();
    public ValueInput<float> Milliseconds = new();
    public ValueInput<float> Microseconds = new();

    public ValueOutput<TimeSpan> Output = new();

    protected override Task Process(PulseContext c)
    {
        var timeSpan = TimeSpan.Zero;
        timeSpan += TimeSpan.FromDays(Days.Read(c));
        timeSpan += TimeSpan.FromHours(Hours.Read(c));
        timeSpan += TimeSpan.FromMinutes(Minutes.Read(c));
        timeSpan += TimeSpan.FromSeconds(Seconds.Read(c));
        timeSpan += TimeSpan.FromMilliseconds(Milliseconds.Read(c));
        timeSpan += TimeSpan.FromMicroseconds(Microseconds.Read(c));
        Output.Write(timeSpan, c);
        return Task.CompletedTask;
    }
}

[Node("TimeSpan Extract", "Date & Time")]
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

[Node("TimeSpan Extract Total", "Date & Time")]
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

[Node("DateTime Construct", "Date & Time")]
public sealed class DateTimeConstructNode : Node
{
    public ValueInput<float> Days = new();
    public ValueInput<float> Hours = new();
    public ValueInput<float> Minutes = new();
    public ValueInput<float> Seconds = new();
    public ValueInput<float> Milliseconds = new();
    public ValueInput<float> Microseconds = new();

    public ValueOutput<DateTime> Output = new();

    protected override Task Process(PulseContext c)
    {
        var dateTime = DateTime.UnixEpoch;
        dateTime += TimeSpan.FromDays(Days.Read(c));
        dateTime += TimeSpan.FromHours(Hours.Read(c));
        dateTime += TimeSpan.FromMinutes(Minutes.Read(c));
        dateTime += TimeSpan.FromSeconds(Seconds.Read(c));
        dateTime += TimeSpan.FromMilliseconds(Milliseconds.Read(c));
        dateTime += TimeSpan.FromMicroseconds(Microseconds.Read(c));
        Output.Write(dateTime, c);
        return Task.CompletedTask;
    }
}

[Node("DateTime Extract", "Date & Time")]
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