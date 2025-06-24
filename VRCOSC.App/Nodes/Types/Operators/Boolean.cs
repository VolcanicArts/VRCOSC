// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("AND", "Operators/Boolean")]
[NodeCollapsed]
public class BooleanAndNode : Node
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) && B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("OR", "Operators/Boolean")]
[NodeCollapsed]
public class BooleanOrNode : Node
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) || B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("NOT", "Operators/Boolean")]
[NodeCollapsed]
public class BooleanNotNode : Node
{
    public ValueInput<bool> Input = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(!Input.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("NAND", "Operators/Boolean")]
[NodeCollapsed]
public class BooleanNandNode : Node
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(!(A.Read(c) && B.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("NOR", "Operators/Boolean")]
[NodeCollapsed]
public class BooleanNorNode : Node
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(!(A.Read(c) || B.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("XOR", "Operators/Boolean")]
[NodeCollapsed]
public class BooleanXorNode : Node
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) ^ B.Read(c), c);
        return Task.CompletedTask;
    }
}

[Node("XNOR", "Operators/Boolean")]
[NodeCollapsed]
public class BooleanXnorNode : Node
{
    public ValueInput<bool> A = new();
    public ValueInput<bool> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(A.Read(c) == B.Read(c), c);
        return Task.CompletedTask;
    }
}