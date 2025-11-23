// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math;

[Node("Clamp", "Math/Clamp")]
public sealed class ClampNode<T> : Node where T : INumber<T>
{
    public ValueInput<T> Value = new();
    public ValueInput<T> Min = new();
    public ValueInput<T> Max = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.Clamp(Value.Read(c), Min.Read(c), Max.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Clamp 0,1", "Math/Clamp")]
[NodeCollapsed]
public sealed class Clamp01Node<T> : Node where T : INumber<T>
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(T.Clamp(Input.Read(c), T.Zero, T.One), c);
        return Task.CompletedTask;
    }
}