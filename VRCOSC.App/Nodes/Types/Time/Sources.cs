// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("DateTime Now", "Date & Time")]
[NodeCollapsed]
public sealed class DateTimeNowSourceNode() : ValueSourceNode<DateTime>(() => DateTime.Now);

[Node("Date Today", "Date & Time")]
[NodeCollapsed]
public sealed class DateTimeTodaySourceNode() : ValueSourceNode<DateTime>(() => DateTime.Today);

[Node("UTC Now", "Date & Time")]
[NodeCollapsed]
public sealed class DateTimeUtcNowSourceNode() : ValueSourceNode<DateTime>(() => DateTime.UtcNow);

[Node("Unix Epoch", "Date & Time")]
public sealed class DateTimeUnixEpochConstantNode() : ConstantNode<DateTime>(DateTime.UnixEpoch);