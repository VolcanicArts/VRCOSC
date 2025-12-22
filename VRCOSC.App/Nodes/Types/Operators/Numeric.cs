// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
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
        var input = Input.Read(c);
        Output.Write(input++, c);
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
        var input = Input.Read(c);
        Output.Write(input--, c);
        return Task.CompletedTask;
    }
}

[Node("Minimum", "Operators/Numeric")]
public sealed class MinimumNode<T> : Node where T : INumber<T>
{
    public ValueInputList<T> Inputs = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        var inputs = Inputs.Read(c);
        Output.Write(inputs.Min()!, c);
        return Task.CompletedTask;
    }
}

[Node("Maximum", "Operators/Numeric")]
public sealed class MaximumNode<T> : Node where T : INumber<T>
{
    public ValueInputList<T> Inputs = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        var inputs = Inputs.Read(c);
        Output.Write(inputs.Max()!, c);
        return Task.CompletedTask;
    }
}

[Node("Floating Point To Number")]
[NodeCollapsed]
public sealed class FloatingPointToNumberNode<Tfp, Tn> : Node where Tfp : IFloatingPoint<Tfp> where Tn : INumber<Tn>
{
    public ValueInput<Tfp> Input = new();
    public ValueOutput<Tn> Output = new();

    protected override Task Process(PulseContext c)
    {
        try
        {
            var input = Input.Read(c);
            var output = Tn.CreateChecked(input);
            Output.Write(output, c);
        }
        catch
        {
            Output.Write(default!, c);
        }

        return Task.CompletedTask;
    }
}