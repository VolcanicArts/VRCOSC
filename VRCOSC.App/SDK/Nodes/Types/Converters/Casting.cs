// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types.Converters;

[Node("Cast", "")]
public sealed class CastNode<TFrom, TTo> : Node
{
    [NodeProcess]
    private void process(TFrom value, ref TTo outValue)
    {
        outValue = (TTo)Convert.ChangeType(value, typeof(TTo))!;
    }
}