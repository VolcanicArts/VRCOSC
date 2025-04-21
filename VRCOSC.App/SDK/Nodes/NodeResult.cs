// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes;

public abstract class NodeResult
{
    public object?[] Values { get; protected init; }
}

public sealed class NodeResult<T1> : NodeResult
{
    public NodeResult(T1 a)
    {
        Values = new object?[1];
        Values[0] = a;
    }
}

public sealed class NodeResult<T1, T2> : NodeResult
{
    public NodeResult(T1 a, T2 b)
    {
        Values = new object?[2];
        Values[0] = a;
        Values[1] = b;
    }
}

public sealed class NodeResult<T1, T2, T3> : NodeResult
{
    public NodeResult(T1 a, T2 b, T3 c)
    {
        Values = new object?[3];
        Values[0] = a;
        Values[1] = b;
        Values[2] = c;
    }
}

public sealed class NodeResult<T1, T2, T3, T4> : NodeResult
{
    public NodeResult(T1 a, T2 b, T3 c, T4 d)
    {
        Values = new object?[4];
        Values[0] = a;
        Values[1] = b;
        Values[2] = c;
        Values[3] = d;
    }
}