// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("DateTime Now", "Date & Time")]
[NodeCollapsed]
public sealed class DateTimeNowSourceNode : UpdateNode<DateTime>
{
    public ValueOutput<DateTime> DateTime = new();

    protected override Task Process(PulseContext c)
    {
        DateTime.Write(System.DateTime.Now, c);
        return Task.CompletedTask;
    }

    protected override DateTime GetValue(PulseContext c) => System.DateTime.Now;
}

[Node("Date Now", "Date & Time")]
[NodeCollapsed]
public sealed class DateNowSourceNode : UpdateNode<DateTime>
{
    public ValueOutput<DateTime> Date = new();

    protected override Task Process(PulseContext c)
    {
        Date.Write(DateTime.Today, c);
        return Task.CompletedTask;
    }

    protected override DateTime GetValue(PulseContext c) => DateTime.Today;
}

[Node("UTC Now", "Date & Time")]
[NodeCollapsed]
public sealed class UTCNowSourceNode : UpdateNode<DateTime>
{
    public ValueOutput<DateTime> UTCNow = new();

    protected override Task Process(PulseContext c)
    {
        UTCNow.Write(DateTime.UtcNow, c);
        return Task.CompletedTask;
    }

    protected override DateTime GetValue(PulseContext c) => DateTime.UtcNow;
}