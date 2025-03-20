// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types;

[NodeFlow(true)]
public class TriggerNode : Node
{
    public TriggerNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private int? execute() => 0;
}

public class ParameterReceivedTriggerNode : TriggerNode
{
    public ParameterReceivedTriggerNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private int execute()
    {
        // if parameter has been received, return 0, else return null
        return 0;
    }
}