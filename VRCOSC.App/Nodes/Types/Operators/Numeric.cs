// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Numerics;
using System.Threading.Tasks;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Add", "Operators/Numeric", EFontAwesomeIcon.Solid_Plus)]
public sealed class AddNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) + B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Subtract", "Operators/Numeric", EFontAwesomeIcon.Solid_Minus)]
public sealed class SubtractNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) - B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Multiply", "Operators/Numeric", EFontAwesomeIcon.Solid_Asterisk)]
public sealed class MultiplyNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) * B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Divide", "Operators/Numeric", EFontAwesomeIcon.Solid_Divide)]
public sealed class DivideNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) / B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Modulo", "Operators/Numeric", EFontAwesomeIcon.Solid_Percent)]
public sealed class ModuloNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) % B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Greater Than", "Operators/Numeric", EFontAwesomeIcon.Solid_GreaterThan)]
public sealed class GreaterThanNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) > B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Greater Than Or Equal", "Operators/Numeric", EFontAwesomeIcon.Solid_GreaterThanEqual)]
public sealed class GreaterThanOrEqualNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) >= B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Less Than", "Operators/Numeric", EFontAwesomeIcon.Solid_LessThan)]
public sealed class LessThanNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) < B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("Less Than Or Equal", "Operators/Numeric", EFontAwesomeIcon.Solid_LessThanEqual)]
public sealed class LessThanOrEqualNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) <= B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("To Bool", "Operators/Numeric")]
[NodeCollapsed]
public sealed class ToBoolNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(Input.Read(c) > T.Zero, c);
        return Task.CompletedTask;
    }
}

[Node("Increment", "Operators/Numeric")]
[NodeCollapsed]
public sealed class IncrementNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(Input.Read(c) + T.One, c);
        return Task.CompletedTask;
    }
}

[Node("Decrement", "Operators/Numeric")]
[NodeCollapsed]
public sealed class DecrementNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(Input.Read(c) - T.One, c);
        return Task.CompletedTask;
    }
}

[Node("Round", "Operators/Numeric")]
[NodeCollapsed]
public sealed class RoundNode<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.Round(Input.Read(c), MidpointRounding.AwayFromZero), c);
        return Task.CompletedTask;
    }
}

[Node("Ceil", "Operators/Numeric")]
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

[Node("Floor", "Operators/Numeric")]
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

[Node("Floating Point To Number", "Operators/Numeric")]
[NodeCollapsed]
public sealed class FloatingPointToNumberNode<Tfp, Tn> : Node where Tfp : IFloatingPoint<Tfp> where Tn : INumber<Tn>
{
    public ValueInput<Tfp> Input = new();
    public ValueOutput<Tn> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        var output = Tn.CreateSaturating(input);
        Output.Write(output, c);
        return Task.CompletedTask;
    }
}

[Node("Delta", "Operators/Numeric")]
[NodeCollapsed]
public sealed class DeltaNode<T> : Node where T : INumber<T>
{
    public GlobalStore<T> PrevValue = new();

    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        var value = Input.Read(c);
        var delta = value - PrevValue.Read(c);
        PrevValue.Write(value, c);
        Output.Write(delta, c);
        return Task.CompletedTask;
    }
}