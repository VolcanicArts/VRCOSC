// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

[Node("Cast")]
public sealed class CastNode<TFrom, TTo> : ValueTransformNode<TFrom, TTo>
{
    private readonly Delegate converter = typeof(TFrom).CreateConverter(typeof(TTo));

    protected override TTo TransformValue(TFrom value, PulseContext c) => (TTo)converter.DynamicInvoke(value)!;
}