// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Negate", "Math")]
[NodeCollapsed]
public class NegateNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.Abs(Input.Read(c)) * -T.One, c);
        return Task.CompletedTask;
    }
}