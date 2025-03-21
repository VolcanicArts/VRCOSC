// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("Branch")]
[NodeFlow(false, 2)]
[NodeValue]
[NodeInputs("Condition")]
public class BranchNode : Node
{
    private const int flow_true_slot = 0;
    private const int flow_false_slot = 1;

    [NodeProcess]
    private int? execute(bool input) => input ? flow_true_slot : flow_false_slot;
}

[Node("Is Equal")]
[NodeValue([typeof(bool)])]
[NodeInputs("", "")]
public class IsEqualNode : Node
{
    [NodeProcess]
    private void process(string? inputA, string? inputB)
    {
        if (inputA is null || inputB is null)
        {
            SetOutput(0, false);
        }

        SetOutput(0, inputA == inputB);
    }

    [NodeProcess]
    private void process(int inputA, int inputB)
    {
        SetOutput(0, inputA == inputB);
    }
}