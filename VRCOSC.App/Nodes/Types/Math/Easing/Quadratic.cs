// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Quadratic In", "Math/Easing")]
[NodeCollapsed]
public sealed class QuadraticInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quadratic.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Quadratic Out", "Math/Easing")]
[NodeCollapsed]
public sealed class QuadraticOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quadratic.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Quadratic InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class QuadraticInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quadratic.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}