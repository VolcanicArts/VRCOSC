// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

public abstract class ResultComputeNode<TLeft, TRight, TResult> : ValueComputeNode<TResult>
{
    public ValueInput<TLeft> A;
    public ValueInput<TRight> B;

    protected ResultComputeNode(string aName = "", string bName = "", string resultName = "")
        : base(resultName)
    {
        A = new ValueInput<TLeft>(string.IsNullOrEmpty(aName) ? "A" : aName);
        B = new ValueInput<TRight>(string.IsNullOrEmpty(bName) ? "B" : bName);
    }

    protected override TResult ComputeValue(PulseContext c)
    {
        var a = A.Read(c);
        var b = B.Read(c);

        return ComputeResult(a, b);
    }

    protected abstract TResult ComputeResult(TLeft a, TRight b);
}

public abstract class ResultComputeNode<TInput, TResult>(string aName = "", string bName = "", string resultName = "") : ResultComputeNode<TInput, TInput, TResult>(aName, bName, resultName);

public abstract class ResultComputeNode<T>(string aName = "", string bName = "", string resultName = "") : ResultComputeNode<T, T>(aName, bName, resultName);

public abstract class SimpleResultComputeNode<TLeft, TRight, TResult>(Func<TLeft, TRight, TResult> func, string aName = "", string bName = "", string resultName = "") : ResultComputeNode<TLeft, TRight, TResult>(aName, bName, resultName)
{
    protected override TResult ComputeResult(TLeft a, TRight b) => func(a, b);
}

public abstract class SimpleResultComputeNode<TInput, TResult>(Func<TInput, TInput, TResult> func, string aName = "", string bName = "", string resultName = "") : ResultComputeNode<TInput, TResult>(aName, bName, resultName)
{
    protected override TResult ComputeResult(TInput a, TInput b) => func(a, b);
}

public abstract class SimpleResultComputeNode<T>(Func<T, T, T> func, string aName = "", string bName = "", string resultName = "") : ResultComputeNode<T>(aName, bName, resultName)
{
    protected override T ComputeResult(T a, T b) => func(a, b);
}

public abstract class TryResultComputeAsyncNode<TLeft, TRight, TResult> : TryValueComputeAsyncNode<TResult>
{
    public ValueInput<TLeft> A;
    public ValueInput<TRight> B;

    protected TryResultComputeAsyncNode(string aName = "", string bName = "", string resultName = "")
        : base(resultName)
    {
        A = new ValueInput<TLeft>(string.IsNullOrEmpty(aName) ? "A" : aName);
        B = new ValueInput<TRight>(string.IsNullOrEmpty(bName) ? "B" : bName);
    }

    protected override async Task<Result<TResult>> TryComputeValueAsync(PulseContext c)
    {
        try
        {
            var a = A.Read(c);
            var b = B.Read(c);
            return await TryComputeResultAsync(a, b, c);
        }
        catch
        {
            return Result<TResult>.Fail();
        }
    }

    protected abstract Task<Result<TResult>> TryComputeResultAsync(TLeft a, TRight b, PulseContext c);
}

public abstract class TryResultComputeNode<TLeft, TRight, TResult>(string aName = "", string bName = "", string resultName = "") : TryResultComputeAsyncNode<TLeft, TRight, TResult>(aName, bName, resultName)
{
    protected override Task<Result<TResult>> TryComputeResultAsync(TLeft a, TRight b, PulseContext c) => Task.FromResult(TryComputeResult(a, b, c));

    protected abstract Result<TResult> TryComputeResult(TLeft a, TRight b, PulseContext c);
}