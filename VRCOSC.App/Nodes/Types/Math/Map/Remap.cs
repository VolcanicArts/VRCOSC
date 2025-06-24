// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap", "Math/Map")]
public class RemapNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Value = new();
    public ValueInput<T> FromMin = new("From Min");
    public ValueInput<T> FromMax = new("From Max");
    public ValueInput<T> ToMin = new("To Min");
    public ValueInput<T> ToMax = new("To Max");
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        var value = double.CreateChecked(Value.Read(c));
        var fromMin = double.CreateChecked(FromMin.Read(c));
        var fromMax = double.CreateChecked(FromMax.Read(c));
        var toMin = double.CreateChecked(ToMin.Read(c));
        var toMax = double.CreateChecked(ToMax.Read(c));

        Result.Write(T.CreateChecked(Utils.Interpolation.Map(value, fromMin, fromMax, toMin, toMax)), c);
        return Task.CompletedTask;
    }
}