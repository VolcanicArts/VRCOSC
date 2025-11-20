// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Damp Continuously", "Math/Interpolation")]
public sealed class DampContinuouslyNode<T> : Node, IActiveUpdateNode where T : IFloatingPointIeee754<T>
{
    public GlobalStore<T> Current = new();

    public ValueInput<T> Target = new();
    public ValueInput<double> HalfTimeMilli = new("Half Time Milli");
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(Current.Read(c), c);
        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c)
    {
        var current = Current.Read(c);
        var result = Utils.Interpolation.DampContinuously(current, Target.Read(c), HalfTimeMilli.Read(c) / 2d, 1d / 100d * 1000d);
        Current.Write(result, c);

        return current != result;
    }
}