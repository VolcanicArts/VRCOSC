// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Logic;

[Node("AND", "Comparison")]
public class BooleanAndNode : Node
{
    [NodeProcess(["A", "B"], ["R"])]
    private bool process(bool a, bool b) => a && b;
}

[Node("OR", "Comparison")]
public class BooleanOrNode : Node
{
    [NodeProcess(["A", "B"], ["R"])]
    private bool process(bool a, bool b) => a || b;
}

[Node("NOT", "Comparison")]
public class BooleanNotNode : Node
{
    [NodeProcess(["A"], ["R"])]
    private bool process(bool a) => !a;
}

[Node("NAND", "Comparison")]
public class BooleanNandNode : Node
{
    [NodeProcess(["A", "B"], ["R"])]
    private bool process(bool a, bool b) => !(a && b);
}

[Node("NOR", "Comparison")]
public class BooleanNorNode : Node
{
    [NodeProcess(["A", "B"], ["R"])]
    private bool process(bool a, bool b) => !(a || b);
}

[Node("XOR", "Comparison")]
public class BooleanXorNode : Node
{
    [NodeProcess(["A", "B"], ["R"])]
    private bool process(bool a, bool b) => a ^ b;
}