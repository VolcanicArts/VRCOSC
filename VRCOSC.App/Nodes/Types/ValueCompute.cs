// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

public abstract class ValueComputeNode<T> : Node
{
    public ValueOutput<T> Result;

    protected ValueComputeNode(string resultName = "")
    {
        Result = new ValueOutput<T>(resultName);
    }

    protected override Task Process(PulseContext c)
    {
        T value = default!;

        try
        {
            value = ComputeValue(c);
        }
        catch
        {
            value = default!;
        }

        Result.Write(value, c);
        return Task.CompletedTask;
    }

    protected abstract T ComputeValue(PulseContext c);
}

public abstract class SimpleValueComputeNode<T>(Func<T> func, string resultName = "") : ValueComputeNode<T>(resultName)
{
    protected override T ComputeValue(PulseContext c) => func();
}

public abstract class ActionValueComputeNode<T>(string resultName = "") : ValueComputeNode<T>(resultName), IFlowInput
{
    public FlowContinuation Next = new();

    protected override async Task Process(PulseContext c)
    {
        await base.Process(c);
        await Next.Execute(c);
    }
}

public abstract class SimpleActionValueComputeNode<T>(Func<T> func, string resultName = "") : ActionValueComputeNode<T>(resultName)
{
    protected override T ComputeValue(PulseContext c) => func();
}

public abstract class TryValueComputeAsyncNode<T> : TryActionAsyncNode
{
    public ValueOutput<T> Result;

    protected TryValueComputeAsyncNode(string resultName = "")
    {
        Result = new ValueOutput<T>(resultName);
    }

    protected override async Task<bool> TryActionAsync(PulseContext c)
    {
        T result = default!;

        try
        {
            var valueResult = await TryComputeValueAsync(c);
            if (!valueResult.IsSuccess) return false;

            result = valueResult.Value;
        }
        catch
        {
            return false;
        }

        Result.Write(result, c);
        return true;
    }

    protected abstract Task<Result<T>> TryComputeValueAsync(PulseContext c);
}

public abstract class TryValueComputeNode<T> : TryValueComputeAsyncNode<T>
{
    protected TryValueComputeNode(string resultName = "")
        : base(resultName)
    {
    }

    protected override Task<Result<T>> TryComputeValueAsync(PulseContext c)
    {
        var result = TryComputeValue(c);
        return Task.FromResult(result);
    }

    protected abstract Result<T> TryComputeValue(PulseContext c);
}