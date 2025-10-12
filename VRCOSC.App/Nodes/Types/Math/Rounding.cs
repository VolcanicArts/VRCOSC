// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Round", "Math")]
public sealed class RoundNode<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Input = new();
    public ValueInput<int> DecimalPlaces = new("Decimal Places", maxDigits<T>());
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        var decimalPlaces = int.Clamp(DecimalPlaces.Read(c), 0, maxDigits<T>());
        Output.Write(T.Round(Input.Read(c), decimalPlaces, MidpointRounding.AwayFromZero), c);
        return Task.CompletedTask;
    }

    private static int maxDigits<Tf>()
    {
        return typeof(Tf) == typeof(float) ? 6 :
            typeof(Tf) == typeof(double) ? 15 :
            typeof(Tf) == typeof(decimal) ? 28 : 0;
    }
}

[Node("Ceil", "Math")]
[NodeCollapsed]
public sealed class CeilingNode<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.Ceiling(Input.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Floor", "Math")]
[NodeCollapsed]
public sealed class FloorNode<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.Floor(Input.Read(c)), c);
        return Task.CompletedTask;
    }
}