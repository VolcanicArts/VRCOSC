// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VRCOSC.App.SDK.Nodes.Types.Base;

[Node("Value", "Values")]
public sealed class ValueNode<T> : Node
{
    public T Value { get; set; } = default!;

    [NodeProcess]
    private void process
    (
        [NodeValue] ref T outValue
    )
    {
        outValue = Value;
    }
}

public sealed class ValueDisplayNode<T> : Node, INotifyPropertyChanged
{
    private T value;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class StartAsyncNode : Node, IFlowInput, IFlowOutput
{
    public NodeFlowRef[] FlowOutputs => [new("Async Continue"), new("Continue")];
}

public abstract class ConstantNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] ref T outValue
    )
    {
        outValue = GetValue();
    }

    protected abstract T GetValue();
}

public abstract class InputNode : Node
{
}

[Node("Cast", "")]
public sealed class CastNode<TFrom, TTo> : Node
{
    [NodeProcess]
    private void process(TFrom value, ref TTo outValue)
    {
        outValue = (TTo)Convert.ChangeType(value, typeof(TTo))!;
    }
}