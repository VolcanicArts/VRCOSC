// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Exp", "Math")]
[NodeCollapsed]
public class ExponentialNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.CreateChecked(System.Math.Exp(double.CreateChecked(Input.Read(c)))), c);
        return Task.CompletedTask;
    }
}