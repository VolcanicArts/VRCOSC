// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("AND", "Operators/Boolean")]
public class BooleanAndNode : Node
{
    [NodeProcess]
    private void process(bool a, bool b, Ref<bool> result) => result.Value = a && b;
}

[Node("OR", "Operators/Boolean")]
public class BooleanOrNode : Node
{
    [NodeProcess]
    private void process(bool a, bool b, Ref<bool> result) => result.Value = a || b;
}

[Node("NOT", "Operators/Boolean")]
public class BooleanNotNode : Node
{
    [NodeProcess]
    private void process(bool a, Ref<bool> result) => result.Value = !a;
}

[Node("NAND", "Operators/Boolean")]
public class BooleanNandNode : Node
{
    [NodeProcess]
    private void process(bool a, bool b, Ref<bool> result) => result.Value = !(a && b);
}

[Node("NOR", "Operators/Boolean")]
public class BooleanNorNode : Node
{
    [NodeProcess]
    private void process(bool a, bool b, Ref<bool> result) => result.Value = !(a || b);
}

[Node("XOR", "Operators/Boolean")]
public class BooleanXorNode : Node
{
    [NodeProcess]
    private void process(bool a, bool b, Ref<bool> result) => result.Value = a ^ b;
}

[Node("XNOR", "Operators/Boolean")]
public class BooleanXNorNode : Node
{
    [NodeProcess]
    private void process(bool a, bool b, Ref<bool> result) => result.Value = a == b;
}