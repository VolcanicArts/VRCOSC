// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Nodes.Types.Base;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("DateTime Now", "DateTime")]
[NodeCollapsed]
public class DateTimeNowSourceNode : SourceNode<DateTime>
{
    public ValueOutput<DateTime> DateTime = new();

    protected override void Process(PulseContext c)
    {
        DateTime.Write(System.DateTime.Now, c);
    }

    protected override DateTime GetValue(PulseContext c) => System.DateTime.Now;
}