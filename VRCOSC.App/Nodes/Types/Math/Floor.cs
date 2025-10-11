// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Floor", "Math")]
[NodeCollapsed]
public sealed class FloorNode<T> : Node where T : IFloatingPoint<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.Floor(Input.Read(c)), c);
        return Task.CompletedTask;
    }
}