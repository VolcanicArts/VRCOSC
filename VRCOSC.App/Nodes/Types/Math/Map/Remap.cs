// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Map;

[Node("Remap", "Math/Map")]
public sealed class RemapNode<TFrom, TTo> : Node where TFrom : INumber<TFrom> where TTo : INumber<TTo>
{
    public ValueInput<TFrom> Value = new();
    public ValueInput<TFrom> FromMin = new("From Min");
    public ValueInput<TFrom> FromMax = new("From Max");
    public ValueInput<TTo> ToMin = new("To Min");
    public ValueInput<TTo> ToMax = new("To Max");
    public ValueOutput<TTo> Result = new();

    protected override Task Process(PulseContext c)
    {
        var value = double.CreateSaturating(Value.Read(c));
        var fromMin = double.CreateSaturating(FromMin.Read(c));
        var fromMax = double.CreateSaturating(FromMax.Read(c));
        var toMin = double.CreateSaturating(ToMin.Read(c));
        var toMax = double.CreateSaturating(ToMax.Read(c));
        Result.Write(TTo.CreateSaturating(Utils.Interpolation.Map(value, fromMin, fromMax, toMin, toMax)), c);

        return Task.CompletedTask;
    }
}

[Node("Remap 0,1 To -1,1", "Math/Map")]
[NodeCollapsed]
public sealed class Remap0111Node<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Value = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        var value = Value.Read(c);
        Result.Write(T.CreateSaturating(Utils.Interpolation.Map(value, T.Zero, T.One, T.NegativeOne, T.One)), c);
        return Task.CompletedTask;
    }
}

[Node("Remap -1,1 To 0,1", "Math/Map")]
[NodeCollapsed]
public sealed class Remap1101Node<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Value = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        var value = Value.Read(c);
        Result.Write(T.CreateSaturating(Utils.Interpolation.Map(value, T.NegativeOne, T.One, T.Zero, T.One)), c);
        return Task.CompletedTask;
    }
}