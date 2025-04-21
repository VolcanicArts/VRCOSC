// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types.Converters;

[Node("Cast", "")]
public sealed class CastNode<TFrom, TTo> : Node
{
    [NodeProcess(["From"], ["To"])]
    private TTo process(TFrom value) => (TTo)Convert.ChangeType(value, typeof(TTo))!;
}