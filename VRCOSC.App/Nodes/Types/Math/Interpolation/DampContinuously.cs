// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Damp Continuously", "Math/Interpolation")]
public sealed class DampContinuouslyNode<T> : Node, IUpdateNode where T : INumber<T>
{
    public GlobalStore<T> Current = new();

    public ValueInput<T> Target = new();
    public ValueInput<float> TimeToTargetMilli = new("Time To Target Milli");
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        var current = double.CreateChecked(Current.Read(c));
        var target = double.CreateChecked(Target.Read(c));
        var length = TimeToTargetMilli.Read(c);

        var result = Utils.Interpolation.DampContinuously(current, target, length / 2f, (1f / 60f) * 1000f);
        var tResult = T.CreateChecked(result);

        Result.Write(tResult, c);
        Current.Write(tResult, c);

        return Task.CompletedTask;
    }

    public bool OnUpdate(PulseContext c) => true;
}