// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VRCOSC.App.Nodes.Types.Utility;

public abstract class DisplayNodeBase<T> : ValueConsumeNode<T>, IDisplayNode, INotifyPropertyChanged
{
    public T Value
    {
        get;
        private set
        {
            if (EqualityComparer<T>.Default.Equals(Value, value)) return;

            field = value;
            OnPropertyChanged();
        }
    } = default!;

    public void Clear() => Value = default!;

    protected override void ConsumeValue(T value, IPulseContext c) => Value = value;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

[Node("Display")]
public sealed class DisplayNode<T> : DisplayNodeBase<T>;

[Node("Passthrough Display", "Utility")]
public sealed class PassthroughDisplayNode<T> : DisplayNodeBase<T>
{
    public ValueOutput<T> Output = new();

    protected override void ConsumeValue(T value, IPulseContext c)
    {
        base.ConsumeValue(value, c);
        Output.Write(value, c);
    }
}