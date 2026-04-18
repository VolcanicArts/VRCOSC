// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Numerics;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Bitwise NOT", "Operators/Bitwise")]
[NodeCollapsed]
public sealed class BitwiseNotNode<T>() : SimpleValueTransformNode<T>(value => ~value) where T : IBitwiseOperators<T, T, T>;

[Node("Bitwise AND", "Operators/Bitwise")]
[NodeCollapsed]
public sealed class BitwiseAndNode<T>() : SimpleResultComputeNode<T>((a, b) => a & b) where T : IBitwiseOperators<T, T, T>;

[Node("Bitwise AND (Multi)", "Operators/Bitwise")]
public sealed class BitwiseMultiAndNode<T> : ValueComputeNode<T> where T : IBitwiseOperators<T, T, T>
{
    public override string DisplayName => "Bitwise AND";

    public ValueInputList<T> Values = new();

    protected override T ComputeValue(PulseContext c)
    {
        var values = Values.Read(c);
        return values.Count != 0 ? values.Aggregate((a, v) => a & v) : default!;
    }
}

[Node("Bitwise NAND", "Operators/Bitwise")]
[NodeCollapsed]
public sealed class BitwiseNandNode<T>() : SimpleResultComputeNode<T>((a, b) => ~(a & b)) where T : IBitwiseOperators<T, T, T>;

[Node("Bitwise NAND (Multi)", "Operators/Bitwise")]
public sealed class BitwiseMultiNandNode<T> : ValueComputeNode<T> where T : IBitwiseOperators<T, T, T>
{
    public override string DisplayName => "Bitwise NAND";

    public ValueInputList<T> Values = new();

    protected override T ComputeValue(PulseContext c)
    {
        var values = Values.Read(c);
        return values.Count != 0 ? ~values.Aggregate((a, v) => a & v) : default!;
    }
}

[Node("Bitwise OR", "Operators/Bitwise")]
[NodeCollapsed]
public sealed class BitwiseOrNode<T>() : SimpleResultComputeNode<T>((a, b) => a | b) where T : IBitwiseOperators<T, T, T>;

[Node("Bitwise OR (Multi)", "Operators/Bitwise")]
public sealed class BitwiseMultiOrNode<T> : ValueComputeNode<T> where T : IBitwiseOperators<T, T, T>
{
    public override string DisplayName => "Bitwise OR";

    public ValueInputList<T> Values = new();

    protected override T ComputeValue(PulseContext c)
    {
        var values = Values.Read(c);
        return values.Count != 0 ? values.Aggregate((a, v) => a | v) : default!;
    }
}

[Node("Bitwise NOR", "Operators/Bitwise")]
[NodeCollapsed]
public sealed class BitwiseNorNode<T>() : SimpleResultComputeNode<T>((a, b) => ~(a | b)) where T : IBitwiseOperators<T, T, T>;

[Node("Bitwise NOR (Multi)", "Operators/Bitwise")]
public sealed class BitwiseMultiNorNode<T> : ValueComputeNode<T> where T : IBitwiseOperators<T, T, T>
{
    public override string DisplayName => "Bitwise NOR";

    public ValueInputList<T> Values = new();

    protected override T ComputeValue(PulseContext c)
    {
        var values = Values.Read(c);
        return values.Count != 0 ? ~values.Aggregate((a, v) => a | v) : default!;
    }
}

[Node("Bitwise XOR", "Operators/Bitwise")]
[NodeCollapsed]
public sealed class BitwiseXorNode<T>() : SimpleResultComputeNode<T>((a, b) => a ^ b) where T : IBitwiseOperators<T, T, T>;

[Node("Bitwise XOR (Multi)", "Operators/Bitwise")]
public sealed class BitwiseMultiXorNode<T> : ValueComputeNode<T> where T : IBitwiseOperators<T, T, T>
{
    public override string DisplayName => "Bitwise XOR";

    public ValueInputList<T> Values = new();

    protected override T ComputeValue(PulseContext c)
    {
        var values = Values.Read(c);
        return values.Count != 0 ? values.Aggregate((a, v) => a ^ v) : default!;
    }
}

[Node("Bitwise XNOR", "Operators/Bitwise")]
[NodeCollapsed]
public sealed class BitwiseXnorNode<T>() : SimpleResultComputeNode<T>((a, b) => ~(a ^ b)) where T : IBitwiseOperators<T, T, T>;

[Node("Bitwise XNOR (Multi)", "Operators/Bitwise")]
public sealed class BitwiseMultiXNorNode<T> : ValueComputeNode<T> where T : IBitwiseOperators<T, T, T>
{
    public override string DisplayName => "Bitwise XNOR";

    public ValueInputList<T> Values = new();

    protected override T ComputeValue(PulseContext c)
    {
        var values = Values.Read(c);
        return values.Count != 0 ? ~values.Aggregate((a, v) => a ^ v) : default!;
    }
}