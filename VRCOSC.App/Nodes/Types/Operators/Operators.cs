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
    [NodeProcess]
    private void process(T a, T b, Ref<T> result) => result.Value = a + b;
}

[Node("Subtract", "Operators", EFontAwesomeIcon.Solid_Minus)]
public sealed class SubtractNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<T> result) => result.Value = a - b;
}

[Node("Multiply", "Operators", EFontAwesomeIcon.Solid_Asterisk)]
public sealed class MultiplyNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<T> result) => result.Value = a * b;
}

[Node("Divide", "Operators", EFontAwesomeIcon.Solid_Divide)]
public sealed class DivideNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<T> result) => result.Value = a / b;
}

[Node("Modolo", "Operators", EFontAwesomeIcon.Solid_Percent)]
public sealed class ModoloNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<T> result) => result.Value = a % b;
}

[Node("Greater Than", "Operators", EFontAwesomeIcon.Solid_GreaterThan)]
public sealed class GreaterThanNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<bool> result) => result.Value = a > b;
}

[Node("Greater Than Or Equal", "Operators", EFontAwesomeIcon.Solid_GreaterThanEqual)]
public sealed class GreaterThanOrEqualToNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<bool> result) => result.Value = a >= b;
}

[Node("Less Than", "Operators", EFontAwesomeIcon.Solid_LessThan)]
public sealed class LessThanNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<bool> result) => result.Value = a < b;
}

[Node("Less Than Or Equal", "Operators", EFontAwesomeIcon.Solid_LessThanEqual)]
public sealed class LessThanOrEqualToNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, Ref<bool> result) => result.Value = a <= b;
}

[Node("Equals", "Operators", EFontAwesomeIcon.Solid_Equals)]
public sealed class EqualsNode<T> : Node
{
    [NodeProcess]
    private void process(T a, T b, Ref<bool> result)
    {
        result.Value = EqualityComparer<T>.Default.Equals(a, b);
    }
}

[Node("Not Equals", "Operators", EFontAwesomeIcon.Solid_NotEqual)]
public sealed class NotEqualsNode<T> : Node
{
    [NodeProcess]
    private void process(T a, T b, Ref<bool> result)
    {
        result.Value = !EqualityComparer<T>.Default.Equals(a, b);
    }
}

[Node("Float To Bool", "Operators")]
public sealed class FloatToBoolNode : Node
{
    [NodeProcess]
    private void process(float input, Ref<bool> result) => result.Value = input > 0;
}

[Node("Int To Bool", "Operators")]
public sealed class IntToBoolNode : Node
{
    [NodeProcess]
    private void process(int input, Ref<bool> result) => result.Value = input > 0;
}