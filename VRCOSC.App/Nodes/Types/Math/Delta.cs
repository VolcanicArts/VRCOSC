// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Delta", "Math")]
[NodeCollapsed]
public class DeltaNode<T> : Node where T : INumber<T>
{
    public GlobalStore<T> PrevValue = new();

    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override void Process(PulseContext c)
    {
        var input = Input.Read(c);
        var prevValue = PrevValue.Read(c);

        Output.Write(prevValue - input, c);
        PrevValue.Write(input, c);
    }
}