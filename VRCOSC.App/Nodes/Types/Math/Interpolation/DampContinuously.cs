// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Interpolation;

[Node("Damp Continuously", "Math/Interpolation")]
public sealed class DampContinuouslyNode : Node
{
    public ValueInput<float> Current = new();
    public ValueInput<float> Target = new();
    public ValueInput<float> LengthMilliseconds = new();
    public ValueInput<float> ElapsedTimeMilliseconds = new();
    public ValueOutput<float> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Utils.Interpolation.DampContinuously(Current.Read(c), Target.Read(c), LengthMilliseconds.Read(c) / 2f, ElapsedTimeMilliseconds.Read(c)), c);
    }
}