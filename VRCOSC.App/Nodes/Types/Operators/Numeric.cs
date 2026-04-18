// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Add", "Operators/Numeric", EFontAwesomeIcon.Solid_Plus)]
public sealed class AddNode<T>() : SimpleResultComputeNode<T>((a, b) => a + b) where T : IAdditionOperators<T, T, T>;

[Node("Subtract", "Operators/Numeric", EFontAwesomeIcon.Solid_Plus)]
public sealed class SubtractNode<T>() : SimpleResultComputeNode<T>((a, b) => a - b) where T : ISubtractionOperators<T, T, T>;

[Node("Multiply", "Operators/Numeric", EFontAwesomeIcon.Solid_Asterisk)]
public sealed class MultiplyNode<T>() : SimpleResultComputeNode<T>((a, b) => a * b) where T : IMultiplyOperators<T, T, T>;

[Node("Divide", "Operators/Numeric", EFontAwesomeIcon.Solid_Divide)]
public sealed class DivideNode<T>() : SimpleResultComputeNode<T>((a, b) => a / b) where T : IDivisionOperators<T, T, T>;

[Node("Modulo", "Operators/Numeric", EFontAwesomeIcon.Solid_Percent)]
public sealed class ModuloNode<T>() : SimpleResultComputeNode<T>((a, b) => a % b) where T : IModulusOperators<T, T, T>;

[Node("Greater Than", "Operators/Numeric", EFontAwesomeIcon.Solid_GreaterThan)]
public sealed class GreaterThanNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a > b) where T : IComparisonOperators<T, T, bool>;

[Node("Greater Than Or Equal", "Operators/Numeric", EFontAwesomeIcon.Solid_GreaterThanEqual)]
public sealed class GreaterThanOrEqualNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a >= b) where T : IComparisonOperators<T, T, bool>;

[Node("Less Than", "Operators/Numeric", EFontAwesomeIcon.Solid_LessThan)]
public sealed class LessThanNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a < b) where T : IComparisonOperators<T, T, bool>;

[Node("Less Than Or Equal", "Operators/Numeric", EFontAwesomeIcon.Solid_LessThanEqual)]
public sealed class LessThanOrEqualNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a <= b) where T : IComparisonOperators<T, T, bool>;

[Node("Increment", "Operators/Numeric")]
[NodeCollapsed]
public sealed class IncrementNode<T>() : SimpleValueTransformNode<T>(v => ++v) where T : IIncrementOperators<T>;

[Node("Decrement", "Operators/Numeric")]
[NodeCollapsed]
public sealed class DecrementNode<T>() : SimpleValueTransformNode<T>(v => --v) where T : IDecrementOperators<T>;

[Node("Minimum", "Operators/Numeric")]
public sealed class MinimumNode<T> : ValueComputeNode<T> where T : IComparisonOperators<T, T, bool>, IMinMaxValue<T>
{
    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(PulseContext c)
    {
        var inputs = Inputs.Read(c);

        var min = T.MaxValue;

        foreach (var v in inputs)
        {
            if (v < min) min = v;
        }

        return min;
    }
}

[Node("Maximum", "Operators/Numeric")]
public sealed class MaximumNode<T> : ValueComputeNode<T> where T : IComparisonOperators<T, T, bool>, IMinMaxValue<T>
{
    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(PulseContext c)
    {
        var inputs = Inputs.Read(c);

        var max = T.MinValue;

        foreach (var v in inputs)
        {
            if (v > max) max = v;
        }

        return max;
    }
}

[Node("To Bool")]
[NodeCollapsed]
public sealed class ToBoolNode<T>() : SimpleValueTransformNode<T, bool>(v => v > T.Zero) where T : INumber<T>;

[Node("Floating Point To Number")]
[NodeCollapsed]
public sealed class FloatingPointToNumberNode<Tfp, Tn> : ValueTransformNode<Tfp, Tn> where Tfp : IFloatingPoint<Tfp> where Tn : INumber<Tn>
{
    protected override Tn TransformValue(Tfp value)
    {
        try
        {
            return Tn.CreateChecked(value);
        }
        catch
        {
            return default!;
        }
    }
}