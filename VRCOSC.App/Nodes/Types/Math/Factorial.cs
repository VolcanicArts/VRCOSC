// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Factorial", "Math")]
[NodeCollapsed]
public class FactorialNode<T> : Node where T : INumber<T>
{
    public ValueInput<int> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        var result = T.One;

        for (var i = 1; i <= Input.Read(c); i++)
        {
            result *= T.CreateSaturating(i);
        }

        Output.Write(result, c);
        return Task.CompletedTask;
    }
}