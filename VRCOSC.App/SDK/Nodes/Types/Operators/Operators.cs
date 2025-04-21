// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Numerics;

namespace VRCOSC.App.SDK.Nodes.Types.Operators;

[Node("+", "Operators")]
public sealed class AddNode<T> : Node where T : INumber<T>
{
    [NodeProcess(["A", "B"], ["R"])]
    private T process(T a, T b) => a + b;
}

[Node("-", "Operators")]
public sealed class SubtractNode<T> : Node where T : INumber<T>
{
    [NodeProcess(["A", "B"], ["R"])]
    private T process(T a, T b) => a - b;
}

[Node("*", "Operators")]
public sealed class MultiplyNode<T> : Node where T : INumber<T>
{
    [NodeProcess(["A", "B"], ["R"])]
    private T process(T a, T b) => a * b;
}

[Node("/", "Operators")]
public sealed class DivideNode<T> : Node where T : INumber<T>
{
    [NodeProcess(["A", "B"], ["R"])]
    private T process(T a, T b) => a / b;
}

[Node("%", "Operators")]
public sealed class ModoloNode<T> : Node where T : INumber<T>
{
    [NodeProcess(["A", "B"], ["R"])]
    private T process(T a, T b) => a % b;
}

[Node("==", "Operators")]
public sealed class EqualsNode<T> : Node
{
    [NodeProcess(["A", "B"], ["R"])]
    private bool process(T a, T b)
    {
        if (a is IEquatable<T> aE) return aE.Equals(b);

        return EqualityComparer<T>.Default.Equals(a, b);
    }
}

[Node("!=", "Operators")]
public sealed class NotEqualsNode<T> : Node
{
    [NodeProcess(["A", "B"], ["R"])]
    private bool process(T a, T b)
    {
        if (a is IEquatable<T> aE) return !aE.Equals(b);

        return !EqualityComparer<T>.Default.Equals(a, b);
    }
}