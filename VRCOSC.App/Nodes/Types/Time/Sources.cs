// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("DateTime Now", "Date & Time")]
[NodeCollapsed]
[NodeForceReprocess]
public sealed class DateTimeNowSourceNode() : SimpleValueSourceNode<DateTime>(() => DateTime.Now);

[Node("Date Today", "Date & Time")]
[NodeCollapsed]
[NodeForceReprocess]
public sealed class DateTimeTodaySourceNode() : SimpleValueSourceNode<DateTime>(() => DateTime.Today);

[Node("UTC Now", "Date & Time")]
[NodeCollapsed]
[NodeForceReprocess]
public sealed class DateTimeUtcNowSourceNode() : SimpleValueSourceNode<DateTime>(() => DateTime.UtcNow);

[Node("Unix Epoch", "Date & Time")]
public sealed class DateTimeUnixEpochConstantNode() : ConstantNode<DateTime>(DateTime.UnixEpoch);