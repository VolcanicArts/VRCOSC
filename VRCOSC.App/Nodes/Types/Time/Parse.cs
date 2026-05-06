// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("Parse DateTime", "Date & Time")]
public sealed class DateTimeParse : TryValueComputeNode<DateTime>
{
    public ValueInput<string> Value = new();
    public ValueInput<DateTimeStyles> Styles = new(defaultValue: DateTimeStyles.AssumeUniversal);

    protected override Result<DateTime> TryComputeValue(PulseContext c) => DateTime.TryParse(Value.Read(c), null, Styles.Read(c), out var dateTime) ? dateTime : Result<DateTime>.Fail();
}