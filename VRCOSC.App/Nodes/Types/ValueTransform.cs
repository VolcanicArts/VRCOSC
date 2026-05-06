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

    protected override TTo ComputeValue(PulseContext c) => TransformValue(Input.Read(c));

    protected abstract TTo TransformValue(TFrom value);
}

public abstract class ValueTransformNode<T>(string inputName = "", string outputName = "") : ValueTransformNode<T, T>(inputName, outputName);

public abstract class SimpleValueTransformNode<TFrom, TTo>(Func<TFrom, TTo> func, string inputName = "", string outputName = "") : ValueTransformNode<TFrom, TTo>(inputName, outputName)
{
    protected override TTo TransformValue(TFrom value) => func(value);
}

public abstract class SimpleValueTransformNode<T>(Func<T, T> func, string inputName = "", string outputName = "") : SimpleValueTransformNode<T, T>(func, inputName, outputName);

public abstract class TryValueTransformAsyncNode<TFrom, TTo> : TryValueComputeAsyncNode<TTo>
{
    public ValueInput<TFrom> Input;

    protected TryValueTransformAsyncNode(string inputName = "", string outputName = "")
        : base(outputName)
    {
        Input = new ValueInput<TFrom>(inputName);
    }

    protected override Task<Result<TTo>> TryComputeValueAsync(PulseContext c) => TryTransformValueAsync(Input.Read(c), c);

    protected abstract Task<Result<TTo>> TryTransformValueAsync(TFrom value, PulseContext c);
}

public abstract class TryValueTransformNode<TFrom, TTo> : TryValueTransformAsyncNode<TFrom, TTo>
{
    protected TryValueTransformNode(string inputName = "", string outputName = "")
        : base(inputName, outputName)
    {
    }

    protected override Task<Result<TTo>> TryTransformValueAsync(TFrom value, PulseContext c)
    {
        var result = TryTransformValue(value, c);
        return Task.FromResult(result);
    }

    protected abstract Result<TTo> TryTransformValue(TFrom value, PulseContext c);
}