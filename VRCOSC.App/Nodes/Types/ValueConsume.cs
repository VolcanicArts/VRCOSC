// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

public abstract class ValueConsumeNode<T> : Node
{
    public ValueInput<T> Input;

    protected ValueConsumeNode(string inputName = "")
    {
        Input = new ValueInput<T>(string.IsNullOrEmpty(inputName) ? "Value" : inputName);
    }

    protected override Task Process(IPulseContext c)
    {
        var value = Input.Read(c);

        try
        {
            ConsumeValue(value, c);
        }
        catch
        {
        }

        return Task.CompletedTask;
    }

    protected abstract void ConsumeValue(T value, IPulseContext c);
}

public abstract class SimpleValueConsumeNode<T>(Action<T> func, string inputName = "") : ValueConsumeNode<T>(inputName)
{
    protected override void ConsumeValue(T value, IPulseContext c) => func(value);
}

public abstract class ActionValueConsumeNode<T>(string inputName = "") : ValueConsumeNode<T>(inputName)
{
    public FlowInput FlowInput = new();
    public FlowOutput Next = new();

    protected override async Task Process(IPulseContext c)
    {
        await base.Process(c);
        await Next.Execute(c);
    }
}

public abstract class SimpleActionValueConsumeNode<T>(Action<T> func, string inputName = "") : ActionValueConsumeNode<T>(inputName)
{
    protected override void ConsumeValue(T value, IPulseContext c) => func(value);
}

public abstract class TryValueConsumeAsyncNode<T> : TryActionAsyncNode
{
    public ValueInput<T> Input;

    protected TryValueConsumeAsyncNode(string inputName = "")
    {
        Input = new ValueInput<T>(inputName);
    }

    protected override async Task<bool> TryActionAsync(IPulseContext c)
    {
        try
        {
            var value = Input.Read(c);
            var valueResult = await TryConsumeValueAsync(value, c);
            return valueResult.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    protected abstract Task<Result> TryConsumeValueAsync(T value, IPulseContext c);
}

public abstract class TryValueConsumeNode<T>(string inputName = "") : TryValueConsumeAsyncNode<T>(inputName)
{
    protected override Task<Result> TryConsumeValueAsync(T value, IPulseContext c) => Task.FromResult(TryConsumeValue(value, c));

    protected abstract Result TryConsumeValue(T value, IPulseContext c);
}