// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Logic;

[Node("AND", "Comparison")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class BooleanAndNode : Node
{
    [NodeProcess]
    private bool process(bool a, bool b) => a && b;
}

[Node("OR", "Comparison")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class BooleanOrNode : Node
{
    [NodeProcess]
    private bool process(bool a, bool b) => a || b;
}

[Node("NOT", "Comparison")]
[NodeValueInput("")]
[NodeValueOutput("")]
public class BooleanNotNode : Node
{
    [NodeProcess]
    private bool process(bool a) => !a;
}

[Node("NAND", "Comparison")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class BooleanNandNode : Node
{
    [NodeProcess]
    private bool process(bool a, bool b) => !(a && b);
}

[Node("NOR", "Comparison")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class BooleanNorNode : Node
{
    [NodeProcess]
    private bool process(bool a, bool b) => !(a || b);
}

[Node("XOR", "Comparison")]
[NodeValueInput("", "")]
[NodeValueOutput("")]
public class BooleanXorNode : Node
{
    [NodeProcess]
    private bool process(bool a, bool b) => a ^ b;
}