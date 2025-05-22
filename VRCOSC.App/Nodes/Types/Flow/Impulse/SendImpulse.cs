// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Impulse;

[Node("Send Impulse", "Flow/Impulse")]
public sealed class SendImpulseNode : Node, IFlowInput
{
    public string? ImpulseName { get; set; }

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrEmpty(ImpulseName)) return;

        NodeField.TriggerImpulse(c, ImpulseName);
    }
}