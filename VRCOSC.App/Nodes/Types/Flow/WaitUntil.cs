// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Wait Until True", "Flow")]
public sealed class WaitUntilTrueNode : Node, IActiveUpdateNode, IFlowInput
{
    public GlobalStore<bool> CurrCondition = new();

    public FlowContinuation Next = new();

    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        while (!c.IsCancelled && !CurrCondition.Read(c))
        {
            await Task.Delay(10);
        }

        await Next.Execute(c);
    }

    public bool OnUpdate(PulseContext c)
    {
        CurrCondition.Write(Condition.Read(c), c);
        return false;
    }
}

[Node("Wait Until False", "Flow")]
public sealed class WaitUntilFalseNode : Node, IActiveUpdateNode, IFlowInput
{
    public GlobalStore<bool> CurrCondition = new();

    public FlowContinuation Next = new();

    public ValueInput<bool> Condition = new();

    protected override async Task Process(PulseContext c)
    {
        while (!c.IsCancelled && CurrCondition.Read(c))
        {
            await Task.Delay(10);
        }

        await Next.Execute(c);
    }

    public bool OnUpdate(PulseContext c)
    {
        CurrCondition.Write(Condition.Read(c), c);
        return false;
    }
}