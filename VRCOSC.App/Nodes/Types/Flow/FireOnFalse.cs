// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On False", "Flow")]
public sealed class FireOnFalseNode : Node
{
    public FlowContinuation Next = new("Next");

    public GlobalStore<bool> PreviousValue = new();

    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        await Next.Execute(c);
    }

    protected override bool ShouldProcess(PulseContext c)
    {
        if (!Condition.Read(c) && PreviousValue.Read(c))
        {
            PreviousValue.Write(Condition.Read(c), c);
            return true;
        }

        PreviousValue.Write(Condition.Read(c), c);
        return false;
    }
}