// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap", "Math/Map")]
public class RemapNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Value = new();
    public ValueInput<T> FromMin = new();
    public ValueInput<T> FromMax = new();
    public ValueInput<T> ToMin = new();
    public ValueInput<T> ToMax = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        var value = double.CreateChecked(Value.Read(c));
        var fromMin = double.CreateChecked(FromMin.Read(c));
        var fromMax = double.CreateChecked(FromMax.Read(c));
        var toMin = double.CreateChecked(ToMin.Read(c));
        var toMax = double.CreateChecked(ToMax.Read(c));

        Result.Write(T.CreateChecked(Utils.Interpolation.Map(value, fromMin, fromMax, toMin, toMax)), c);
    }
}