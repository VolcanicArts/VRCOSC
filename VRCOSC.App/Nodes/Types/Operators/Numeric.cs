// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Operators;

#region Add

[Node("Add", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Plus)]
public class AddNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a + b) where TLeft : IAdditionOperators<TLeft, TRight, TResult> where TRight : notnull;

public class AddNode<TLeft, TRight> : AddNode<TLeft, TRight, TLeft> where TLeft : IAdditionOperators<TLeft, TRight, TLeft> where TRight : notnull;

public sealed class AddNode<T> : AddNode<T, T> where T : IAdditionOperators<T, T, T>;

[Node("Add (Multi)", "Operators/Numeric")]
public sealed class MultiAddNode<T> : ValueComputeNode<T> where T : IAdditionOperators<T, T, T>
{
    public override string DisplayName => "Add";

    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(IPulseContext c) => Inputs.Read(c).Aggregate((curr, next) => curr + next);
}

#endregion

#region Subtract

[Node("Subtract", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Minus)]
public class SubtractNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a - b) where TLeft : ISubtractionOperators<TLeft, TRight, TResult> where TRight : notnull;

public class SubtractNode<TLeft, TRight> : SubtractNode<TLeft, TRight, TLeft> where TLeft : ISubtractionOperators<TLeft, TRight, TLeft> where TRight : notnull;

public sealed class SubtractNode<T> : SubtractNode<T, T> where T : ISubtractionOperators<T, T, T>;

[Node("Subtract (Multi)", "Operators/Numeric")]
public sealed class MultiSubtractNode<T> : ValueComputeNode<T> where T : ISubtractionOperators<T, T, T>
{
    public override string DisplayName => "Subtract";

    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(IPulseContext c) => Inputs.Read(c).Aggregate((curr, next) => curr - next);
}

#endregion

#region Multiply

[Node("Multiply", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Asterisk)]
public class MultiplyNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a * b) where TLeft : IMultiplyOperators<TLeft, TRight, TResult> where TRight : notnull;

public class MultiplyNode<TLeft, TRight> : MultiplyNode<TLeft, TRight, TLeft> where TLeft : IMultiplyOperators<TLeft, TRight, TLeft> where TRight : notnull;

public sealed class MultiplyNode<T> : MultiplyNode<T, T> where T : IMultiplyOperators<T, T, T>;

[Node("Multiply (Multi)", "Operators/Numeric")]
public sealed class MultiMultiplyNode<T> : ValueComputeNode<T> where T : IMultiplyOperators<T, T, T>
{
    public override string DisplayName => "Multiply";

    public ValueInputList<T> Inputs = new();

    protected override T ComputeValue(IPulseContext c) => Inputs.Read(c).Aggregate((curr, next) => curr * next);
}

#endregion

#region Divide

[Node("Divide", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Divide)]
public class DivideNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a / b) where TLeft : IDivisionOperators<TLeft, TRight, TResult> where TRight : notnull;

public class DivideNode<TLeft, TRight> : DivideNode<TLeft, TRight, TLeft> where TLeft : IDivisionOperators<TLeft, TRight, TLeft> where TRight : notnull;

public sealed class DivideNode<T> : DivideNode<T, T> where T : IDivisionOperators<T, T, T>;

#endregion

#region Modulo

[Node("Modulo", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Percent)]
public class ModuloNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a % b) where TLeft : IModulusOperators<TLeft, TRight, TResult> where TRight : notnull;

public class ModuloNode<TLeft, TRight> : ModuloNode<TLeft, TRight, TLeft> where TLeft : IModulusOperators<TLeft, TRight, TLeft> where TRight : notnull;

public sealed class ModuloNode<T> : ModuloNode<T, T> where T : IModulusOperators<T, T, T>;

#endregion

#region Greater Than

[Node("Greater Than", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_GreaterThan)]
public class GreaterThanNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a > b) where TLeft : IComparisonOperators<TLeft, TRight, TResult> where TRight : notnull;

public class GreaterThanNode<TLeft, TRight> : GreaterThanNode<TLeft, TRight, bool> where TLeft : IComparisonOperators<TLeft, TRight, bool> where TRight : notnull;

public sealed class GreaterThanNode<T> : GreaterThanNode<T, T> where T : IComparisonOperators<T, T, bool>;

#endregion

#region Greater Than Or Equal

[Node("Greater Than Or Equal", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_GreaterThanEqual)]
public class GreaterThanOrEqualNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a >= b) where TLeft : IComparisonOperators<TLeft, TRight, TResult> where TRight : notnull;

public class GreaterThanOrEqualNode<TLeft, TRight> : GreaterThanOrEqualNode<TLeft, TRight, bool> where TLeft : IComparisonOperators<TLeft, TRight, bool> where TRight : notnull;

public sealed class GreaterThanOrEqualNode<T> : GreaterThanOrEqualNode<T, T> where T : IComparisonOperators<T, T, bool>;

#endregion

#region Less Than

[Node("Less Than", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_LessThan)]
public class LessThanNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a < b) where TLeft : IComparisonOperators<TLeft, TRight, TResult> where TRight : notnull;

public class LessThanNode<TLeft, TRight> : LessThanNode<TLeft, TRight, bool> where TLeft : IComparisonOperators<TLeft, TRight, bool> where TRight : notnull;

public sealed class LessThanNode<T> : LessThanNode<T, T> where T : IComparisonOperators<T, T, bool>;

#endregion

#region Less Than Or Equal

[Node("Less Than Or Equal", "Operators/Numeric")]
[NodeCollapsed(EFontAwesomeIcon.Solid_LessThanEqual)]
public class LessThanOrEqualNode<TLeft, TRight, TResult>() : SimpleResultComputeNode<TLeft, TRight, TResult>((a, b) => a <= b) where TLeft : IComparisonOperators<TLeft, TRight, TResult> where TRight : notnull;

public class LessThanOrEqualNode<TLeft, TRight> : LessThanOrEqualNode<TLeft, TRight, bool> where TLeft : IComparisonOperators<TLeft, TRight, bool> where TRight : notnull;

public sealed class LessThanOrEqualNode<T> : LessThanOrEqualNode<T, T> where T : IComparisonOperators<T, T, bool>;

#endregion

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

[Node("Velocity", "Operators/Numeric")]
[NodeCollapsed]
public sealed class VelocityNode<T> : Node, IContinuousNode where T : IFloatingPoint<T>
{
    public int UpdateOffset => 1;

    public GlobalStore<T> PrevValue = new();

    public ValueInput<T> Input = new();
    public ValueOutput<T> Velocity = new();

    protected override Task Process(IPulseContext c)
    {
        var current = Input.Read(c);
        var deltaTimeSeconds = c.DeltaTime / 1000d;
        var prev = PrevValue.Read(c);
        var delta = current - prev;
        var velocity = double.CreateChecked(delta) / deltaTimeSeconds;

        Velocity.Write(T.CreateChecked(velocity), c);
        PrevValue.Write(current, c);
        return Task.CompletedTask;
    }
}

[Node("To Bool")]
[NodeCollapsed]
public sealed class ToBoolNode<T>() : SimpleValueTransformNode<T, bool>(v => v > T.Zero) where T : INumber<T>;

[Node("Floating Point To Number")]
[NodeCollapsed]
public sealed class FloatingPointToNumberNode<Tfp, Tn>() : SimpleValueTransformNode<Tfp, Tn>(Tn.CreateChecked) where Tfp : IFloatingPoint<Tfp> where Tn : INumber<Tn>;