// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("NOT", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanNotNode() : SimpleValueTransformNode<bool>(v => !v);

[Node("AND", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanAndNode() : SimpleResultComputeNode<bool>((a, b) => a && b);

[Node("AND (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiAndNode : ValueComputeNode<bool>
{
    public override string DisplayName => "AND";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => Values.Read(c).All(v => v);
}

[Node("OR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanOrNode() : SimpleResultComputeNode<bool>((a, b) => a || b);

[Node("OR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiOrNode : ValueComputeNode<bool>
{
    public override string DisplayName => "OR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => Values.Read(c).Any(v => v);
}

[Node("NAND", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanNandNode() : SimpleResultComputeNode<bool>((a, b) => !(a && b));

[Node("NAND (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiNandNode : ValueComputeNode<bool>
{
    public override string DisplayName => "NAND";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => !Values.Read(c).All(v => v);
}

[Node("NOR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanNorNode() : SimpleResultComputeNode<bool>((a, b) => !(a || b));

[Node("NOR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiNorNode : ValueComputeNode<bool>
{
    public override string DisplayName => "NOR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => !Values.Read(c).Any(v => v);
}

[Node("XOR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanXorNode() : SimpleResultComputeNode<bool>((a, b) => a ^ b);

[Node("XOR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiXorNode : ValueComputeNode<bool>
{
    public override string DisplayName => "XOR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => Values.Read(c).Aggregate((curr, value) => curr ^ value);
}

[Node("XNOR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanXnorNode() : SimpleResultComputeNode<bool>((a, b) => !(a ^ b));

[Node("XNOR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiXnorNode : ValueComputeNode<bool>
{
    public override string DisplayName => "XNOR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => !Values.Read(c).Aggregate((curr, value) => curr ^ value);
}