// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

public abstract class AsyncActionNode : Node
{
    public FlowInput FlowInput = new();
    public FlowOutput Next = new();

    protected override async Task Process(IPulseContext c)
    {
        await DoActionAsync(c);
        await Next.Execute(c);
    }

    protected abstract Task DoActionAsync(IPulseContext c);
}

public abstract class SimpleAsyncActionNode(Func<Task> action) : AsyncActionNode
{
    protected override Task DoActionAsync(IPulseContext c) => action();
}

public abstract class ActionNode : AsyncActionNode
{
    protected override Task DoActionAsync(IPulseContext c)
    {
        DoAction(c);
        return Task.CompletedTask;
    }

    protected abstract void DoAction(IPulseContext c);
}

public abstract class SimpleActionNode(Action action) : ActionNode
{
    protected override void DoAction(IPulseContext c) => action();
}

public abstract class TryActionAsyncNode : Node
{
    public FlowInput FlowInput = new();
    public FlowOutput OnSuccess = new();
    public FlowOutput OnFail = new();

    protected override async Task Process(IPulseContext c)
    {
        bool result = false;

        try
        {
            result = await TryActionAsync(c);
        }
        catch
        {
            result = false;
        }

        if (result)
            await OnSuccess.Execute(c);
        else
            await OnFail.Execute(c);
    }

    protected abstract Task<bool> TryActionAsync(IPulseContext c);
}

public abstract class TryActionNode : TryActionAsyncNode
{
    protected override Task<bool> TryActionAsync(IPulseContext c)
    {
        var result = TryAction(c);
        return Task.FromResult(result);
    }

    protected abstract bool TryAction(IPulseContext c);
}