// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Numerics;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Add", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Plus)]
public class AddNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a + b) where TLeft : IAdditionOperators<TLeft, TRight, TResult> where TRight : notnull;

public class AddNode<TInput, TResult> : AddNode<TInput, TInput, TResult> where TInput : IAdditionOperators<TInput, TInput, TResult>;

public sealed class AddNode<T> : AddNode<T, T> where T : IAdditionOperators<T, T, T>;

[Node("Add (Multi)", "Operators/Numeric")]
public sealed class MultiAddNode<T> : ValueComputeNode<T> where T : IAdditionOperators<T, T, T>
{
    public override string DisplayName => "Add";

    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(IPulseContext c) => Inputs.Read(c).Aggregate((curr, next) => curr + next);
}

[Node("Subtract", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Minus)]
public sealed class SubtractNode<T>() : SimpleResultComputeNode<T>((a, b) => a - b) where T : ISubtractionOperators<T, T, T>;

[Node("Subtract (Multi)", "Operators/Numeric")]
public sealed class MultiSubtractNode<T> : ValueComputeNode<T> where T : ISubtractionOperators<T, T, T>
{
    public override string DisplayName => "Subtract";

    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(IPulseContext c) => Inputs.Read(c).Aggregate((curr, next) => curr - next);
}

[Node("Multiply", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Asterisk)]
public sealed class MultiplyNode<T>() : SimpleResultComputeNode<T>((a, b) => a * b) where T : IMultiplyOperators<T, T, T>;

[Node("Multiply (Multi)", "Operators/Numeric")]
public sealed class MultiMultiplyNode<T> : ValueComputeNode<T> where T : IMultiplyOperators<T, T, T>
{
    public override string DisplayName => "Multiply";

    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(IPulseContext c) => Inputs.Read(c).Aggregate((curr, next) => curr * next);
}

[Node("Divide", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Divide)]
public sealed class DivideNode<T>() : SimpleResultComputeNode<T>((a, b) => a / b) where T : IDivisionOperators<T, T, T>;

[Node("Modulo", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Percent)]
public sealed class ModuloNode<T>() : SimpleResultComputeNode<T>((a, b) => a % b) where T : IModulusOperators<T, T, T>;

[Node("Greater Than", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_GreaterThan)]
public sealed class GreaterThanNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a > b) where T : IComparisonOperators<T, T, bool>;

[Node("Greater Than Or Equal", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_GreaterThanEqual)]
public sealed class GreaterThanOrEqualNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a >= b) where T : IComparisonOperators<T, T, bool>;

[Node("Less Than", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_LessThan)]
public sealed class LessThanNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a < b) where T : IComparisonOperators<T, T, bool>;

[Node("Less Than Or Equal", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_LessThanEqual)]
public sealed class LessThanOrEqualNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => a <= b) where T : IComparisonOperators<T, T, bool>;

[Node("Increment", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Plus, EFontAwesomeIcon.Solid_1)]
public sealed class IncrementNode<T>() : SimpleValueTransformNode<T>(v => ++v) where T : IIncrementOperators<T>;

[Node("Decrement", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Minus, EFontAwesomeIcon.Solid_1)]
public sealed class DecrementNode<T>() : SimpleValueTransformNode<T>(v => --v) where T : IDecrementOperators<T>;

[Node("Minimum", "Operators/Numeric")]
public sealed class MinimumNode<T> : ValueComputeNode<T> where T : IComparisonOperators<T, T, bool>, IMinMaxValue<T>
{
    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(IPulseContext c)
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

    protected override T ComputeValue(IPulseContext c)
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
public sealed class FloatingPointToNumberNode<Tfp, Tn>() : SimpleValueTransformNode<Tfp, Tn>(Tn.CreateChecked) where Tfp : IFloatingPoint<Tfp> where Tn : INumber<Tn>;