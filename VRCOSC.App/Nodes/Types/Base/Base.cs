// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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