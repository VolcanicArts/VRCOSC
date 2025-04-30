// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Base;

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

public abstract class ConstantNode<T> : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] Ref<T> outValue
    )
    {
        outValue.Value = GetValue();
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
    private void process(TFrom value, Ref<TTo> outValue)
    {
        outValue.Value = (TTo)Convert.ChangeType(value, typeof(TTo))!;
    }
}