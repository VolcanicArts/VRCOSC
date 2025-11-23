// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

[Node("Cast")]
public sealed class CastNode<TFrom, TTo> : Node where TFrom : IConvertible
{
    public ValueInput<TFrom> Input = new();
    public ValueOutput<TTo> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write((TTo)Convert.ChangeType(Input.Read(c), typeof(TTo)), c);
        return Task.CompletedTask;
    }
}