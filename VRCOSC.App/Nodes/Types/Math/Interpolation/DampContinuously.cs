// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Damp Continuously", "Math/Interpolation")]
public sealed class DampContinuouslyNode<T> : Node, IUpdateNode where T : IFloatingPointIeee754<T>
{
    public GlobalStore<T> Current = new();

    public ValueInput<T> Target = new();
    public ValueInput<double> HalfTimeMilli = new("Half Time Milli");
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        var result = Utils.Interpolation.DampContinuously(Current.Read(c), Target.Read(c), HalfTimeMilli.Read(c), 1d / 60d * 1000d);

        Result.Write(result, c);
        Current.Write(result, c);

        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}