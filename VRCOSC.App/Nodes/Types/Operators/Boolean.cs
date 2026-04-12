// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("AND", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanAndNode : ValueComputeNode<bool>
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();

    protected override bool ComputeValue(PulseContext c) => A.Read(c) && B.Read(c);
}

[Node("AND (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiAndNode : ValueComputeNode<bool>
{
    public override string DisplayName => "AND";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => Values.Read(c).All(v => v);
}

[Node("OR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanOrNode : ValueComputeNode<bool>
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();

    protected override bool ComputeValue(PulseContext c) => A.Read(c) || B.Read(c);
}

[Node("OR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiOrNode : ValueComputeNode<bool>
{
    public override string DisplayName => "OR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => Values.Read(c).Any(v => v);
}

[Node("NOT", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanNotNode : ValueComputeNode<bool>
{
    public ValueInput<bool> Input = new();

    protected override bool ComputeValue(PulseContext c) => !Input.Read(c);
}

[Node("NAND", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanNandNode : ValueComputeNode<bool>
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();

    protected override bool ComputeValue(PulseContext c) => !(A.Read(c) && B.Read(c));
}

[Node("NAND (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiNandNode : ValueComputeNode<bool>
{
    public override string DisplayName => "NAND";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => !Values.Read(c).All(v => v);
}

[Node("NOR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanNorNode : ValueComputeNode<bool>
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();

    protected override bool ComputeValue(PulseContext c) => !(A.Read(c) || B.Read(c));
}

[Node("NOR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiNorNode : ValueComputeNode<bool>
{
    public override string DisplayName => "NOR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => !Values.Read(c).Any(v => v);
}

[Node("XOR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanXorNode : ValueComputeNode<bool>
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();

    protected override bool ComputeValue(PulseContext c) => A.Read(c) ^ B.Read(c);
}

[Node("XOR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiXorNode : ValueComputeNode<bool>
{
    public override string DisplayName => "XOR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => Values.Read(c).Aggregate((curr, value) => curr ^ value);
}

[Node("XNOR", "Operators/Boolean")]
[NodeCollapsed]
public sealed class BooleanXnorNode : ValueComputeNode<bool>
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();

    protected override bool ComputeValue(PulseContext c) => !(A.Read(c) ^ B.Read(c));
}

[Node("XNOR (Multi)", "Operators/Boolean")]
public sealed class BooleanMultiXnorNode : ValueComputeNode<bool>
{
    public override string DisplayName => "XNOR";

    public ValueInputList<bool> Values = new();

    protected override bool ComputeValue(PulseContext c) => !Values.Read(c).Aggregate((curr, value) => curr ^ value);
}