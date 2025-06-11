// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On True", "Flow")]
public sealed class FireOnTrueNode : Node
{
    public FlowCall Next = new("Next");

    public GlobalStore<bool> PreviousValue = new();

    [NodeReactive]
    public ValueInput<bool> Condition = new();

    protected override void Process(PulseContext c)
    {
        Next.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        if (Condition.Read(c) && !PreviousValue.Read(c))
        {
            PreviousValue.Write(Condition.Read(c), c);
            return true;
        }

        PreviousValue.Write(Condition.Read(c), c);
        return false;
    }
}