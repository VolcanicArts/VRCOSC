// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow.Triggers;

[Node("Fire On True", "Flow")]
public sealed class FireOnTrueNode : Node
{
    public FlowCall Next = new("Next");

    public GlobalStore<bool> PreviousValue = new();

    [NodeReactive]
    public ValueInput<bool> Input = new();

    protected override void Process(PulseContext c)
    {
        Next.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        if (Input.Read(c) && !PreviousValue.Read(c))
        {
            PreviousValue.Write(Input.Read(c), c);
            return true;
        }

        PreviousValue.Write(Input.Read(c), c);
        return false;
    }
}