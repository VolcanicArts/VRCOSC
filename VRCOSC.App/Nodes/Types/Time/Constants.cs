// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Nodes.Types.Base;

namespace VRCOSC.App.Nodes.Types.Time;

[Node("Current DateTime", "DateTime")]
[NodeCollapsed]
public class DateTimeNowConstant : ConstantNode<DateTime>
{
    protected override DateTime GetValue() => DateTime.Now;
}