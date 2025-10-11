// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Delta", "Math")]
[NodeCollapsed]
public sealed class DeltaNode<T> : Node where T : INumber<T>
{
    public GlobalStore<T> PrevValue = new();

    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        var prevValue = PrevValue.Read(c);

        Output.Write(input - prevValue, c);
        PrevValue.Write(input, c);
        return Task.CompletedTask;
    }
}