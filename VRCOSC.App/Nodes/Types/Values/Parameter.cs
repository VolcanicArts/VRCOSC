// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes.Types.Values;

[Node("Parameter Source", "Values")]
public sealed class ReceivedParameterSourceNode : Node, IAnyParameterReceiver
{
    private ReceivedParameter? parameter;

    public ReceivedParameter? Parameter
    {
        get => parameter;
        set
        {
            parameter = value;
            NodeField.WalkForward(this);
        }
    }

    public string ParameterName { get; set; } = string.Empty;

    public void OnAnyParameterReceived(ReceivedParameter incomingParameter)
    {
        if (string.IsNullOrEmpty(ParameterName)) return;
        if (incomingParameter.Name != ParameterName) return;

        Parameter = incomingParameter;
    }

    [NodeProcess]
    private void process
    (
        [NodeValue("Parameter")] Ref<ReceivedParameter?> outParameter
    )
    {
        outParameter.Value = Parameter;
    }
}