// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

public abstract class ValueTransformNode<TFrom, TTo> : ValueComputeNode<TTo>
{
    public ValueInput<TFrom> Input;

    protected ValueTransformNode(string inputName = "", string outputName = "")
        : base(outputName)
    {
        Input = new ValueInput<TFrom>(inputName);
    }

    protected override TTo ComputeValue(IPulseContext c) => TransformValue(Input.Read(c), c);

    protected abstract TTo TransformValue(TFrom value, IPulseContext c);
}

public abstract class ValueTransformNode<T>(string inputName = "", string outputName = "") : ValueTransformNode<T, T>(inputName, outputName);

public abstract class SimpleValueTransformNode<TFrom, TTo>(Func<TFrom, TTo> func, string inputName = "", string outputName = "") : ValueTransformNode<TFrom, TTo>(inputName, outputName)
{
    protected override TTo TransformValue(TFrom value, IPulseContext c) => func(value);
}

public abstract class SimpleValueTransformNode<T>(Func<T, T> func, string inputName = "", string outputName = "") : SimpleValueTransformNode<T, T>(func, inputName, outputName);

public abstract class ActionValueTransformNode<T>(string inputName = "", string outputName = "") : ValueTransformNode<T>(inputName, outputName)
{
    public FlowInput FlowInput = new();
    public FlowOutput Next = new();

    protected override async Task Process(IPulseContext c)
    {
        await base.Process(c);
        await Next.Execute(c);
    }
}

public abstract class SimpleActionValueTransformNode<T>(Func<T, T> func, string inputName = "", string outputName = "") : ActionValueTransformNode<T>(inputName, outputName)
{
    protected override T TransformValue(T value, IPulseContext c) => func(value);
}

public abstract class ActionValueTransformNode<TFrom, TTo>(string inputName = "", string outputName = "") : ValueTransformNode<TFrom, TTo>(inputName, outputName)
{
    public FlowInput FlowInput = new();
    public FlowOutput Next = new();

    protected override async Task Process(IPulseContext c)
    {
        await base.Process(c);
        await Next.Execute(c);
    }
}

public abstract class SimpleActionValueTransformNode<TFrom, TTo>(Func<TFrom, TTo> func, string inputName = "", string outputName = "") : ActionValueTransformNode<TFrom, TTo>(inputName, outputName)
{
    protected override TTo TransformValue(TFrom value, IPulseContext c) => func(value);
}

public abstract class TryValueTransformAsyncNode<TFrom, TTo> : TryValueComputeAsyncNode<TTo>
{
    public ValueInput<TFrom> Input;

    protected TryValueTransformAsyncNode(string inputName = "", string outputName = "")
        : base(outputName)
    {
        Input = new ValueInput<TFrom>(inputName);
    }

    protected override Task<Result<TTo>> TryComputeValueAsync(IPulseContext c) => TryTransformValueAsync(Input.Read(c), c);

    protected abstract Task<Result<TTo>> TryTransformValueAsync(TFrom value, IPulseContext c);
}

public abstract class TryValueTransformNode<TFrom, TTo>(string inputName = "", string outputName = "") : TryValueTransformAsyncNode<TFrom, TTo>(inputName, outputName)
{
    protected override Task<Result<TTo>> TryTransformValueAsync(TFrom value, IPulseContext c)
    {
        var result = TryTransformValue(value, c);
        return Task.FromResult(result);
    }

    protected abstract Result<TTo> TryTransformValue(TFrom value, IPulseContext c);
}