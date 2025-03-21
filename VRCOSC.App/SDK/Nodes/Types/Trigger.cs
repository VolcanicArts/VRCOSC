// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types;

[Node("Trigger")]
[NodeFlow(true)]
public class TriggerNode : Node
{
    [NodeProcess]
    private int? execute() => 0;
}

[NodeFlow(true, 3)]
public class ParameterReceivedTriggerNode : TriggerNode
{
    [NodeProcess]
    private int execute()
    {
        var isValid = false;
        var justBecameValid = false;
        var justBecameInvalid = false;

        // if parameter has been received, return 0, else return null
        return 0;
    }
}