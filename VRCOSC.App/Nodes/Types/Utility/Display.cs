// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Display", "Utility")]
public sealed class DisplayNode<T> : Node, IDisplayNode, INotifyPropertyChanged
{
    public T Value
    {
        get;
        set
        {
            if (EqualityComparer<T>.Default.Equals(value, field)) return;

            field = value;
            OnPropertyChanged();
        }
    } = default!;

    public ValueInput<T> Input = new();

    protected override Task Process(PulseContext c)
    {
        Value = Input.Read(c);
        return Task.CompletedTask;
    }

    public void Clear() => Value = default!;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

[Node("Passthrough Display", "Utility")]
public sealed class PassthroughDisplayNode<T> : Node, IDisplayNode, INotifyPropertyChanged
{
    public T Value
    {
        get;
        set
        {
            if (EqualityComparer<T>.Default.Equals(value, field)) return;

            field = value;
            OnPropertyChanged();
        }
    } = default!;

    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        Value = input;
        Output.Write(input, c);
        return Task.CompletedTask;
    }

    public void Clear() => Value = default!;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}