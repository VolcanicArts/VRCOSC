// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

public abstract class ActionNode : Node, IFlowInput
{
    public FlowContinuation Next = new();

    protected override async Task Process(PulseContext c)
    {
        await DoTask(c);
        await Next.Execute(c);
    }

    protected abstract Task DoTask(PulseContext c);
}

public abstract class SimpleActionNode : ActionNode
{
    protected override Task DoTask(PulseContext c)
    {
        DoAction(c);
        return Task.CompletedTask;
    }

    protected abstract void DoAction(PulseContext c);
}