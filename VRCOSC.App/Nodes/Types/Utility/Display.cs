// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.App.Nodes.Types.Utility;

public abstract class DisplayNodeBase<T> : ValueConsumeNode<T>, IDisplayNode
{
    public T Value
    {
        get;
        private set
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;

            field = value;
            OnValueChanged?.Invoke(value);
        }
    } = default!;

    public Action<object?>? OnValueChanged { get; set; }

    public object? GetValue() => Value;

    public void Clear() => Value = default!;

    protected override void ConsumeValue(T value, IPulseContext c)
    {
        Value = value;
    }
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

[Node("Flow Display")]
public sealed class FlowDisplayNode : ActionNode
{
    public event Action? OnCall;

    protected override void DoAction(IPulseContext c) => OnCall?.Invoke();
}