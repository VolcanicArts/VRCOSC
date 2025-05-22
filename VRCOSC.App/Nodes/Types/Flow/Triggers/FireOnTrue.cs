// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire On True", "Flow")]
public sealed class FireOnTrueNode : Node
{
    public FlowCall OnTrue = new("On True");

    [NodeReactive]
    public ValueInput<bool> Input = new();

    protected override void Process(PulseContext c)
    {
        OnTrue.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        return Input.Read(c);
    }
}