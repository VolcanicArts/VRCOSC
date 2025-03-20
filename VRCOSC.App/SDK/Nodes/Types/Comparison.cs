// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types;

[NodeFlow(false, 2)]
[NodeValue]
public class BranchNode : Node
{
    private const int flow_true_slot = 0;
    private const int flow_false_slot = 1;

    public BranchNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private int? execute(bool input) => input ? flow_true_slot : flow_false_slot;

    [NodeProcess]
    private int? execute(int input) => input > 0 ? flow_true_slot : flow_false_slot;

    [NodeProcess]
    private int? execute(float input) => input > 0 ? flow_true_slot : flow_false_slot;
}

[NodeValue([typeof(bool)])]
public class IsEqualNode : Node
{
    public IsEqualNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void process(string? inputA, string? inputB)
    {
        if (inputA is null || inputB is null)
        {
            SetOutputValue(0, false);
        }

        SetOutputValue(0, inputA == inputB);
    }
}