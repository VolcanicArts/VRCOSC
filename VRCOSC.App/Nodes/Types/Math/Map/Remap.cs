// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap", "Math/Map")]
public class RemapNode : Node
{
    public ValueInput<float> Value = new();
    public ValueInput<float> FromMin = new();
    public ValueInput<float> FromMax = new();
    public ValueInput<float> ToMin = new();
    public ValueInput<float> ToMax = new();
    public ValueOutput<float> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Utils.Interpolation.Map(Value.Read(c), FromMin.Read(c), FromMax.Read(c), ToMin.Read(c), ToMax.Read(c)), c);
    }
}