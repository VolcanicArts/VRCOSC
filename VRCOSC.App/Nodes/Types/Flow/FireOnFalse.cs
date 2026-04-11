// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Fire On False", "Flow")]
public sealed class FireOnFalseNode : Node
{
    public FlowContinuation Next = new("Next");

    public GlobalStore<bool> PrevCondition = new();

    public ValueInput<bool> Condition = new();

    protected override Task Process(PulseContext c) => Next.Execute(c);

    protected override bool ShouldProcess(PulseContext c)
    {
        var condition = Condition.Read(c);
        var prevCondition = PrevCondition.Read(c);

        if (!condition && prevCondition)
        {
            PrevCondition.Write(condition, c);
            return true;
        }

        PrevCondition.Write(condition, c);
        return false;
    }
}