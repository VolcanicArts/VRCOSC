// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types;

[Node("Cast")]
public sealed class CastNode<TFrom, TTo> : Node
{
    public ValueInput<TFrom> Input = new();
    public ValueOutput<TTo> Output = new();

    private readonly Delegate converter = typeof(TFrom).CreateConverter(typeof(TTo));

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        Output.Write((TTo)converter.DynamicInvoke(input)!, c);
        return Task.CompletedTask;
    }
}