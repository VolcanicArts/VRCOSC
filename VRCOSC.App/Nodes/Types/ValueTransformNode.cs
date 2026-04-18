// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

public abstract class ValueTransformNode<TFrom, TTo> : Node
{
    public ValueInput<TFrom> Input;
    public ValueOutput<TTo> Output;

    protected ValueTransformNode(string inputName = "", string outputName = "")
    {
        Input = new ValueInput<TFrom>(inputName);
        Output = new ValueOutput<TTo>(outputName);
    }

    protected override Task Process(PulseContext c)
    {
        Output.Write(TransformValue(Input.Read(c)), c);
        return Task.CompletedTask;
    }

    protected abstract TTo TransformValue(TFrom value);
}

public abstract class ValueTransformNode<T> : ValueTransformNode<T, T>;

public abstract class SimpleValueTransformNode<TFrom, TTo> : ValueTransformNode<TFrom, TTo>
{
    private readonly Func<TFrom, TTo> _func;

    protected SimpleValueTransformNode(Func<TFrom, TTo> func, string inputName = "", string outputName = "")
        : base(inputName, outputName)
    {
        _func = func;
    }

    protected override TTo TransformValue(TFrom value) => _func(value);
}

public abstract class SimpleValueTransformNode<T>(Func<T, T> func) : SimpleValueTransformNode<T, T>(func);