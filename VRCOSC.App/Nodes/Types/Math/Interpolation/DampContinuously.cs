// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Damp Continuously", "Math/Interpolation")]
public sealed class DampContinuouslyNode<T> : ValueComputeNode<T>, IContinuousNode where T : IFloatingPointIeee754<T>
{
    public int UpdateOffset => 0;
    public GlobalStore<T> Current = new();

    [InputMode(InputModes.Connection)]
    public ValueInput<T> Target = new();

    public ValueInput<int> HalfTime = new("Half Time (ms)");

    protected override T ComputeValue(IPulseContext c)
    {
        var result = Utils.Interpolation.DampContinuously(Current.Read(c), Target.Read(c), HalfTime.Read(c) / 2d, 1d / 100d * 1000d);
        if (T.IsNaN(result)) result = T.Zero;

        Current.Write(result, c);
        return result;
    }
}