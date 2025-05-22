// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Numerics;
using FontAwesome6;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Add", "Operators", EFontAwesomeIcon.Solid_Plus)]
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

[Node("Subtract", "Operators", EFontAwesomeIcon.Solid_Minus)]
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

[Node("Multiply", "Operators", EFontAwesomeIcon.Solid_Asterisk)]
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

[Node("Divide", "Operators", EFontAwesomeIcon.Solid_Divide)]
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

[Node("Modulo", "Operators", EFontAwesomeIcon.Solid_Percent)]
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

[Node("Greater Than", "Operators", EFontAwesomeIcon.Solid_GreaterThan)]
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

[Node("Greater Than Or Equal", "Operators", EFontAwesomeIcon.Solid_GreaterThanEqual)]
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

[Node("Less Than", "Operators", EFontAwesomeIcon.Solid_LessThan)]
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

[Node("Less Than Or Equal", "Operators", EFontAwesomeIcon.Solid_LessThanEqual)]
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

[Node("Equals", "Operators", EFontAwesomeIcon.Solid_Equals)]
public sealed class EqualsNode<T> : Node
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(EqualityComparer<T>.Default.Equals(A.Read(c), B.Read(c)), c);
    }
}

[Node("Not Equals", "Operators", EFontAwesomeIcon.Solid_NotEqual)]
public sealed class NotEqualsNode<T> : Node
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(!EqualityComparer<T>.Default.Equals(A.Read(c), B.Read(c)), c);
    }
}

[Node("Float To Bool", "Operators")]
[NodeCollapsed]
public sealed class FloatToBoolNode : Node
{
    public ValueInput<float> Input = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c) > 0f, c);
    }
}

[Node("Int To Bool", "Operators")]
[NodeCollapsed]
public sealed class IntToBoolNode : Node
{
    public ValueInput<int> Input = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c) > 0, c);
    }
}

[Node("Increment", "Operators")]
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