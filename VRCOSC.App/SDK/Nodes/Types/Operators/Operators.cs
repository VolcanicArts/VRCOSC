// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Numerics;

namespace VRCOSC.App.SDK.Nodes.Types.Operators;

[Node("+", "Operators")]
public sealed class AddNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref T result) => result = a + b;
}

[Node("-", "Operators")]
public sealed class SubtractNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref T result) => result = a - b;
}

[Node("*", "Operators")]
public sealed class MultiplyNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref T result) => result = a * b;
}

[Node("/", "Operators")]
public sealed class DivideNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref T result) => result = a / b;
}

[Node("%", "Operators")]
public sealed class ModoloNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref T result) => result = a % b;
}

[Node(">", "Operators")]
public sealed class GreaterThanNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref bool result) => result = a > b;
}

[Node(">=", "Operators")]
public sealed class GreaterThanOrEqualToNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref bool result) => result = a >= b;
}

[Node("<", "Operators")]
public sealed class LessThanNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref bool result) => result = a < b;
}

[Node("<=", "Operators")]
public sealed class LessThanOrEqualToNode<T> : Node where T : INumber<T>
{
    [NodeProcess]
    private void process(T a, T b, ref bool result) => result = a <= b;
}

[Node("==", "Operators")]
public sealed class EqualsNode<T> : Node
{
    [NodeProcess]
    private void process(T a, T b, ref bool outResult)
    {
        outResult = a is IEquatable<T> aE ? aE.Equals(b) : EqualityComparer<T>.Default.Equals(a, b);
    }
}

[Node("!=", "Operators")]
public sealed class NotEqualsNode<T> : Node
{
    [NodeProcess]
    private void process(T a, T b, ref bool outResult)
    {
        outResult = a is IEquatable<T> aE ? !aE.Equals(b) : !EqualityComparer<T>.Default.Equals(a, b);
    }
}

[Node("Float To Bool", "Operators")]
public sealed class FloatToBoolNode : Node
{
    [NodeProcess]
    private void process(float input, ref bool outResult) => outResult = input > 0;
}

[Node("Int To Bool", "Operators")]
public sealed class IntToBoolNode : Node
{
    [NodeProcess]
    private void process(int input, ref bool outResult) => outResult = input > 0;
}