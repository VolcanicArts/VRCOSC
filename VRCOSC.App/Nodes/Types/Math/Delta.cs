// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Delta", "Math")]
[NodeCollapsed]
public sealed class DeltaNode<T> : ValueTransformNode<T> where T : ISubtractionOperators<T, T, T>
{
    public GlobalStore<T> PrevValue = new();

    protected override T TransformValue(T value, PulseContext c)
    {
        var prevValue = PrevValue.Read(c);
        PrevValue.Write(value, c);
        return value - prevValue;
    }
}