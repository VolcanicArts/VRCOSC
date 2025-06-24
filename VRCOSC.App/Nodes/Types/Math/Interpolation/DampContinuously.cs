// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Damp Continuously", "Math/Interpolation")]
public sealed class DampContinuouslyNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Current = new();
    public ValueInput<T> Target = new();
    public ValueInput<float> LengthMilliseconds = new();
    public ValueInput<float> ElapsedTimeMilliseconds = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(T.CreateChecked(Utils.Interpolation.DampContinuously(double.CreateChecked(Current.Read(c)), double.CreateChecked(Target.Read(c)), LengthMilliseconds.Read(c) / 2f, ElapsedTimeMilliseconds.Read(c))), c);
        return Task.CompletedTask;
    }
}