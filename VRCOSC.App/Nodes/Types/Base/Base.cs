// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Base;

[NodeCollapsed]
public abstract class ConstantNode<T> : Node
{
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(GetValue(), c);
    }

    protected abstract T GetValue();
}

[Node("Cast", "")]
public sealed class CastNode<TFrom, TTo> : Node
{
    public ValueInput<TFrom> Input = new();
    public ValueOutput<TTo> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write((TTo)Convert.ChangeType(Input.Read(c), typeof(TTo))!, c);
    }
}

[Node("Display", "")]
public sealed class DisplayNode<T> : UpdateNode<T>, INotifyPropertyChanged
{
    private T value;

    public T Value
    {
        get => value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(value, this.value)) return;

            this.value = value;
            OnPropertyChanged();
        }
    }

    public ValueInput<T> Input = new();

    protected override void Process(PulseContext c)
    {
    }

    // janky, but it's internal so it's fine
    protected override T GetValue(PulseContext c)
    {
        Value = Input.Read(c);
        return default!;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}