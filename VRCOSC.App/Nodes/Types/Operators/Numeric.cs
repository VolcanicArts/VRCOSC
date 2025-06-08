// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using FontAwesome6;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Add", "Operators/Numeric", EFontAwesomeIcon.Solid_Plus)]
public sealed class AddNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) + B.Read(c), c);
    }
}

[Node("Subtract", "Operators/Numeric", EFontAwesomeIcon.Solid_Minus)]
public sealed class SubtractNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) - B.Read(c), c);
    }
}

[Node("Multiply", "Operators/Numeric", EFontAwesomeIcon.Solid_Asterisk)]
public sealed class MultiplyNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) * B.Read(c), c);
    }
}

[Node("Divide", "Operators/Numeric", EFontAwesomeIcon.Solid_Divide)]
public sealed class DivideNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) / B.Read(c), c);
    }
}

[Node("Modulo", "Operators/Numeric", EFontAwesomeIcon.Solid_Percent)]
public sealed class ModuloNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<T> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) % B.Read(c), c);
    }
}

[Node("Greater Than", "Operators/Numeric", EFontAwesomeIcon.Solid_GreaterThan)]
public sealed class GreaterThanNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) > B.Read(c), c);
    }
}

[Node("Greater Than Or Equal", "Operators/Numeric", EFontAwesomeIcon.Solid_GreaterThanEqual)]
public sealed class GreaterThanOrEqualNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) >= B.Read(c), c);
    }
}

[Node("Less Than", "Operators/Numeric", EFontAwesomeIcon.Solid_LessThan)]
public sealed class LessThanNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) < B.Read(c), c);
    }
}

[Node("Less Than Or Equal", "Operators/Numeric", EFontAwesomeIcon.Solid_LessThanEqual)]
public sealed class LessThanOrEqualNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(A.Read(c) <= B.Read(c), c);
    }
}

[Node("To Bool", "Operators/Numeric")]
[NodeCollapsed]
public sealed class ToBoolNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c) > T.Zero, c);
    }
}

[Node("Increment", "Operators/Numeric")]
[NodeCollapsed]
public sealed class IncrementNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(Input.Read(c) + T.One, c);
    }
}

[Node("Decrement", "Operators/Numeric")]
[NodeCollapsed]
public sealed class DecrementNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(Input.Read(c) - T.One, c);
    }
}