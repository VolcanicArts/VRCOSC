// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Base;

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

public abstract class SourceNode<T> : Node, INodeSource
{
    private GlobalStore<T> prevValue = new();

    public bool HasChanged(PulseContext c)
    {
        var value = GetValue(c);

        if (!EqualityComparer<T>.Default.Equals(value, prevValue.Read(c)))
        {
            prevValue.Write(value, c);
            return true;
        }

        return false;
    }

    protected abstract T GetValue(PulseContext c);
}